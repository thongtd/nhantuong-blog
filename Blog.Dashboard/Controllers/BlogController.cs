using AutoMapper;
using MicroBase.Share.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroBase.Dashboard.Robo.Filters;
using MicroBase.Dashboard.Robo.Controllers.RoboForm;
using MicroBase.Dashboard.Robo.Attributes;
using MicroBase.Dashboard.Share.Models;
using MicroBase.Dashboard.Robo.Models;
using MicroBase.FileManager;
using MicroBase.Dashboard.Share.Models.RoboForm.UI;
using MicroBase.Dashboard.Robo.Code;
using Microsoft.AspNetCore.Mvc.Rendering;
using MicroBase.Repository.Repositories;
using MicroBase.RedisProvider;
using Blog.Share.Models;
using Blog.Entity;
using Blog.Entity.Entities;
using Blog.Share.Models.CMS;
using Blog.Service.API;
using Blog.Service.CMS;

namespace Blog.Dashboard.Controllers
{
    [Route("dashboard/manage-blogs")]
    [DashboardActionFilter(IndexPageTile = "Bài viết", EditPageTile = "Cập nhật", CreatePageTile = "Thêm mới")]
    [PermissionFilter(Code = "BLOG_CONTROLLER", Name = "Bài viết", Route = "dashboard/manage-blogs")]
    public class BlogController : BaseRoboController<Blog.Entity.Entities.Blog, int, BlogController, BlogModel, BlogDbContext>
    {
        private readonly ILogger<BlogController> logger;
        private readonly IBlogCMSService blogCMSService;
        private readonly IBlogCategoryService blogCategoryService;
        private readonly IBlogCategoryMapService blogCategoryMapService;
        private readonly IFileUploadService fileUploadService;
        private readonly ITagService tagService;
        private readonly IRepository<Blog.Entity.Entities.Blog, BlogDbContext> blogRepo;
        private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;
        private readonly IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo;
        private readonly BlogDbContext microDbContext;
        private readonly IMapper mapper;
        private readonly IRedisStogare redisStogare;
        private readonly IBlogCacheService blogCacheService;

        protected override string IndexFormTitle { get => "Bài viết"; }

        protected override string CreateFormTitle { get => "Bài viết - Thêm mới"; }

        protected override string EditFormTitle { get => "Bài viết - Cập nhật"; }

        protected override string SubmitFormUrl { get; set; }

        protected override string ViewOnlyFormTitle { get => "Bài viết - Xem"; }

        public BlogController(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IRepository<Blog.Entity.Entities.Blog, BlogDbContext> repository,
            ILogger<BlogController> logger,
            IMapper mapper,
            BlogDbContext microDbContext,
            ITagService tagService,
            IBlogCategoryService blogCategoryService,
            IBlogCategoryMapService blogCategoryMapService,
            IBlogCMSService blogCMSService,
            IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo,
            IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo,
            FileUploadFactory fileUploadFactory,
            IRedisStogare redisStogare,
            IBlogCacheService blogCacheService)
            : base(configuration, httpContextAccessor, httpClientFactory, repository, logger, mapper)
        {
            this.logger = logger;
            this.blogCategoryService = blogCategoryService;
            this.blogCMSService = blogCMSService;
            this.blogRepo = repository;
            this.tagService = tagService;
            this.blogCategoryMapService = blogCategoryMapService;
            this.microDbContext = microDbContext;
            this.blogCategoryMapRepo = blogCategoryMapRepo;
            this.mapper = mapper;
            this.blogTagMapRepo = blogTagMapRepo;
            var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
            this.fileUploadService = fileUploadFactory.GetServiceByName(uploadServiceName);
            this.redisStogare = redisStogare;
            this.blogCacheService = blogCacheService;
        }

        protected override void ConvertFromModel(BlogModel model, Blog.Entity.Entities.Blog entity)
        {
            if (model.Id.HasValue && model.Id.Value != 0)
            {
                entity.Id = model.Id.Value;
            }

            entity.Name = model.Name;
            entity.Slug = !string.IsNullOrWhiteSpace(model.Slug) ? model.Slug : model.Name.ToSlugUrl();
            entity.Enabled = model.Enabled;
            entity.HotNews = model.HotNews;
            entity.BodyContent = model.BodyContent;
            entity.View = model.View ?? 0;
            entity.SubContent = model.SubContent;
            entity.Description = model.Description;
            entity.Title = model.Title;
            entity.Keyword = model.Keyword;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.CreatedDate = DateTime.UtcNow;

            if (model.LogoFile != null)
            {
                var uploadRes = fileUploadService.UploadImageAsync(model.LogoFile, "blogs").Result;
                if (uploadRes.Success)
                {
                    entity.Thumbnail = uploadRes.Data.FileUrl;
                }
            }
            else
            {
                entity.Thumbnail = model.ThumbnailUrl;
            }
        }

        protected void ConvertFromEntity(BlogModel model, Blog.Entity.Entities.Blog entity)
        {
            model.Id = entity.Id;
            model.Name = entity.Name;
            model.Slug = entity.Slug;
            model.Enabled = entity.Enabled;
            model.ThumbnailUrl = entity.Thumbnail;
            model.HotNews = entity.HotNews;
            model.BodyContent = entity.BodyContent;
            model.SubContent = entity.SubContent;
            model.Description = entity.Description;
            model.Title = entity.Title;
            model.View = (int)entity.View;
            model.HotNews = model.HotNews;
            model.Enabled = model.Enabled;
        }

        protected override BlogModel GetDefaultModel()
        {
            return new BlogModel();
        }

        protected override Blog.Entity.Entities.Blog GetDefaultEntity()
        {
            return new Entity.Entities.Blog();
        }

        public virtual Task OnCreatingAsync(IActionResult createForm)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnEditingAsync(IActionResult createForm, Blog.Entity.Entities.Blog blog)
        {
            return Task.CompletedTask;
        }

        [HttpGet, Route("get-available")]
        [PermissionFilter(Code = Permission.Role.View, Name = Permission.RoleLabel.View, Route = "get-available")]
        public async Task<IActionResult> GetAvailable(KendoGridFilterModel model, string searchTerm)
        {
            var queryDictionary = QueryHelpers.ParseQuery(HttpContext.Request.QueryString.Value);
            var filter = KendoGridFilterParse.Parse(queryDictionary);

            var predicate = LinqExtensions.BuildPredicate(filter, new Blog.Entity.Entities.Blog());
            var source = await blogCMSService.GetAvailableAsync(searchTerm, predicate, filter.Page, filter.PageSize);
            return Json(new
            {
                data = source.Source,
                total = source.TotalRecords
            });
        }

        [PermissionFilter(Code = Permission.Role.View, Name = Permission.RoleLabel.View, Route = "index")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Blog = "active";
            ViewBag.ActiveLink = "Blog";
            base.RoboFormData = new RoboFormModel
            {
                Title = IndexFormTitle,
                IndexFormSubTitle = "Danh sách bài viết",
                RoboFormType = RoboFormType.GirdForm,
                RoboGirdModel = new RoboGirdModel
                {
                    GridHeader = new GridHeader
                    {
                        GridHeaderFields = new List<GridHeaderField>
                        {
                            new GridHeaderField
                            {
                                Label = "Tìm kiếm",
                                PlaceHolder = "Tìm kiếm theo tiêu đề, slug",
                                Col = 3,
                                Name = "searchTerm",
                                RoboTextType = RoboTextType.TextBox,
                                PreFix = "fa fa-search"
                            }
                        },
                        RoboButtons = new List<RoboButton>
                        {
                            new RoboButton
                            {
                                Name = "search",
                                Label = " Tìm kiếm",
                                Action = ButtonAction.GridSearch
                            },
                        }
                    },
                    RoboGirdFields = new List<RoboGirdField> {
                        new RoboGirdField
                        {
                            Title = "STT",
                            FieldName = "Order",
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                            Width = 50,
                            TextAlign = TextAlign.Center
                        },
                        new RoboGirdField
                        {
                            Title = "TIÊU ĐỀ",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.Name),
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                        },
                        new RoboGirdField
                        {
                            Title = "SLUG",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.Slug),
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                        },
                        new RoboGirdField
                        {
                            Title = "NHÓM TIN",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.CategoryName),
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                        },
                        new RoboGirdField
                        {
                            Title = "ẢNH",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.Thumbnail),
                            RoboGirdFieldType = RoboGirdFieldType.Image,
                            Width = 150,
                            TextAlign = TextAlign.Center,
                        },
                        new RoboGirdField
                        {
                            Title = "LƯỢT XEM",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.View),
                            RoboGirdFieldType = RoboGirdFieldType.Number,
                            Width = 100,
                            TextAlign = TextAlign.Center,
                        },
                        new RoboGirdField
                        {
                            Title = "NỔI BẬT",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.HotNews),
                            RoboGirdFieldType = RoboGirdFieldType.Boolean,
                            Width = 100
                        },
                        new RoboGirdField
                        {
                            Title = "TRẠNG THÁI",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogDetailsResponse>(s => s.Enabled),
                            RoboGirdFieldType = RoboGirdFieldType.Boolean,
                        },
                        new RoboGirdField
                        {
                            Title = "NGÀY TẠO",
                            FieldName = ObjectExtension.GetRoboFieldName<TagCmsResponse>(s => s.CreatedDate),
                            RoboGirdFieldType = RoboGirdFieldType.DateTime,
                        },
                    },
                    FetchDataUrl = Url.Action("GetAvailable"),
                    CreateButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeXLarge,
                        Label = "Thêm mới",
                        Link = "/dashboard/manage-blogs/create"
                    },
                    ActiveUrl = "/dashboard/manage-blogs/active",
                    DeleteUrl = "/dashboard/manage-blogs/delete",
                    EditButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeXLarge,
                        Label = "Cập nhật",
                        Link = "/dashboard/manage-blogs/create"
                    },
                    ViewButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeXLarge,
                        Label = "Details",
                        Link = "/dashboard/manage-blogs/view-only"
                    },
                }
            };

            var roboForm = await BuildRoboFormAsync(null);
            return roboForm;
        }

        [Route("create/{entityId?}")]
        [PermissionFilter(Code = Permission.Role.Create, Name = Permission.RoleLabel.Create, Route = "create")]
        public override async Task<IActionResult> CreateAsync(int? entityId)
        {
            ViewBag.Blog = "active";
            SubmitFormUrl = $"/dashboard/manage-blogs/create/{entityId}";

            return await OnCreateAsync(entityId);
        }

        public override async Task<IActionResult> OnCreateAsync(int? entityId)
        {
            var model = new BlogModel();
            var entity = new Blog.Entity.Entities.Blog();

            if (entityId.HasValue)
            {
                entity = await blogRepo.GetByIdAsync(entityId.Value);
                ConvertFromEntity(model, entity);
                RoboFormData.Title = EditFormTitle;
                RoboFormData.FormSubmitUrl = SubmitFormUrl;
                RoboFormData.CreateButton = new RoboButton
                {
                    Label = "Lưu"
                };
            }
            else
            {
                model = GetDefaultModel();

                RoboFormData.Title = CreateFormTitle;
                RoboFormData.FormSubmitUrl = SubmitFormUrl;
                RoboFormData.CreateButton = new RoboButton
                {
                    Label = "Lưu"
                };
            }

            // Lấy dữ liệu cho Select2 Category
            var selectedCategoryIds = new List<int>();

            if (entityId.HasValue)
            {
                selectedCategoryIds = blogCategoryMapRepo.FindAsync(b => b.IsDelete == false && b.BlogId == entityId.Value).Result
                    .Select(b => b.BlogCategoryId)
                    .ToList();
            }

            var blogCategories = await blogCategoryService.GetRecordsAsync(b => b.IsDelete == false && b.Enabled == true);

            RoboFormData.AutoCompleteDropdowns.Add("BlogCategory", blogCategories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = selectedCategoryIds.Contains(c.Id)
            }));

            // Lấy dữ liệu cho Select2 Tag
            var selectedTagIds = new List<int>();

            if (entityId.HasValue)
            {
                selectedTagIds = blogTagMapRepo.FindAsync(b => b.IsDelete == false && b.BlogId == entityId.Value).Result
                    .Select(b => b.TagId)
                    .ToList();
            }

            var blogTags = await tagService.GetRecordsAsync(b => b.IsDelete == false && b.Enabled == true);

            RoboFormData.AutoCompleteDropdowns.Add("Tag", blogTags.Select(c => new SelectListItem
            {
                Text = c.Keyword,
                Value = c.Id.ToString(),
                Selected = selectedTagIds.Contains(c.Id)
            }));

            var createForm = await BuildRoboFormAsync(model);

            if (entityId.HasValue && entityId.Value != 0)
            {
                RoboFormData.FormActionType = FormActionType.Edit;
                await OnEditingAsync(createForm, entity);
            }
            else
            {
                RoboFormData.FormActionType = FormActionType.Create;
                await OnCreatingAsync(createForm);
            }

            return createForm;
        }

        [HttpPost, Route("create/{entityId?}")]
        [PermissionFilter(Code = Permission.Role.Create, Name = Permission.RoleLabel.Create, Route = "create")]
        public override async Task<IActionResult> OnSubmitSaveChangeAsync(int? entityId, BlogModel model)
        {
            var result = await base.OnSaveChangeAsync(entityId, model);

            if (result is JsonResult jsonResult)
            {
                var baseCmsResponse = jsonResult.Value as BaseCmsResponse<object>;
                int? blogId = null;

                if (baseCmsResponse?.Data is Blog.Entity.Entities.Blog entity)
                {
                    blogId = entity.Id;

                    // ----------- Category -----------
                    var existingCategoryMaps = await blogCategoryMapRepo
                        .FindAsync(c => c.IsDelete == false && c.BlogId == blogId);

                    var existingCategoryIds = existingCategoryMaps.Select(c => c.BlogCategoryId).ToList();
                    var newCategoryIds = model.BlogCategory.Select(c => int.Parse(c)).ToList();

                    foreach (var existingCategoryMap in existingCategoryMaps)
                    {
                        if (!newCategoryIds.Contains(existingCategoryMap.BlogCategoryId))
                        {
                            await blogCategoryMapRepo.DeleteAsync(existingCategoryMap);
                        }
                    }

                    foreach (var newCategoryId in newCategoryIds)
                    {
                        if (!existingCategoryIds.Contains(newCategoryId))
                        {
                            await blogCategoryMapRepo.InsertAsync(new BlogCategoryMap
                            {
                                BlogId = blogId.Value,
                                BlogCategoryId = newCategoryId,
                                Enabled = true,
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }

                    // -------------------- Tag -----------------------
                    var existingTagMaps = await blogTagMapRepo
                        .FindAsync(t => t.IsDelete == false && t.BlogId == blogId);

                    var existingTagIds = existingTagMaps.Select(t => t.TagId).ToList();
                    var newTagIds = model.Tag.Select(t => int.Parse(t)).ToList();

                    foreach (var existingTagMap in existingTagMaps)
                    {
                        if (!newTagIds.Contains(existingTagMap.TagId))
                        {
                            await blogTagMapRepo.DeleteAsync(existingTagMap);
                        }
                    }

                    foreach (var newTagId in newTagIds)
                    {
                        if (!existingTagIds.Contains(newTagId))
                        {
                            await blogTagMapRepo.InsertAsync(new BlogTagMap
                            {
                                BlogId = blogId.Value,
                                TagId = newTagId,
                                Enabled = true,
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }

                await blogCacheService.BuildBlogsToCacheAsync(blogId);
                baseCmsResponse.Data = null;
            }

            return result;
        }

        [HttpPost, Route("delete")]
        [PermissionFilter(Code = Permission.Role.Delete, Name = Permission.RoleLabel.Delete, Route = "delete")]
        public override async Task<IActionResult> OnDeleteAsync(int id)
        {
            var response = await base.DeleteAsync(id);
            return response;
        }

        [Route("get-text-in-form-edit")]
        public async Task<object> GetTextInForm(int blogId)
        {
            var categories = await blogCMSService.GetTextInFormAsync(blogId);
            return categories;
        }

        [Route("view-only/{blogId?}")]
        [PermissionFilter(Code = Permission.Role.Create, Name = Permission.RoleLabel.Create, Route = "create")]
        public override async Task<IActionResult> ViewOnlyAsync(int blogId)
        {
            var model = new BlogModel();
            var entity = new Blog.Entity.Entities.Blog();

            entity = await blogRepo.GetByIdAsync(blogId);
            model = mapper.Map<BlogModel>(entity);
            model.ThumbnailUrl = entity.Thumbnail;
            RoboFormData.Title = ViewOnlyFormTitle;
            RoboFormData.IndexFormTitle = ViewOnlyFormTitle;
            RoboFormData.FormActionType = FormActionType.ViewOnly;

            // Lấy dữ liệu cho Select2 Category
            var selectedCategoryIds = new List<int>();

            selectedCategoryIds = blogCategoryMapRepo.FindAsync(b => b.IsDelete == false && b.BlogId == blogId).Result
                .Select(b => b.BlogCategoryId)
                .ToList();

            var blogCategories = await blogCategoryService.GetRecordsAsync(b => b.IsDelete == false);

            RoboFormData.AutoCompleteDropdowns.Add("BlogCategory", blogCategories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = selectedCategoryIds.Contains(c.Id)
            }));

            // Lấy dữ liệu cho Select2 Tag
            var selectedTagIds = new List<int>();

            selectedTagIds = blogTagMapRepo.FindAsync(b => b.IsDelete == false && b.BlogId == blogId).Result
                .Select(b => b.TagId)
                .ToList();

            var blogTags = await tagService.GetRecordsAsync(b => b.IsDelete == false);

            RoboFormData.AutoCompleteDropdowns.Add("Tag", blogTags.Select(c => new SelectListItem
            {
                Text = c.Keyword,
                Value = c.Id.ToString(),
                Selected = selectedTagIds.Contains(c.Id)
            }));

            var createForm = await BuildRoboFormAsync(model);
            return createForm;
        }

        [Route("add-blog-tag")]
        public async Task<IActionResult> AddBlogTag([FromBody] BlogTag model)
        {
            model.Slug = model.Keyword.ToSlugUrl();
            await tagService.InsertAsync(model);
            return Ok(new
            {
                Data = model
            });
        }
    }
}