using AutoMapper;
using MicroBase.Share.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroBase.Share;
using MicroBase.Dashboard.Robo.Controllers.RoboForm;
using MicroBase.Dashboard.Robo.Attributes;
using MicroBase.FileManager;
using MicroBase.Dashboard.Share.Models.RoboForm.UI;
using MicroBase.Dashboard.Share.Models;
using MicroBase.Dashboard.Robo.Code;
using MicroBase.Dashboard.Robo.Filters;
using MicroBase.Repository.Repositories;
using MicroBase.Share.Models.Base;
using Blog.Entity.Entities;
using Blog.Share.Models;
using Blog.Entity;
using Blog.Service.API;

namespace Blog.Dashboard.Controllers
{
    [Route("dashboard/blog-category-manage")]
    [DashboardActionFilter(IndexPageTile = "Danh mục bài viết", EditPageTile = "Cập nhật", CreatePageTile = "Thêm mới")]
    [PermissionFilter(Code = "BLOG_CATEGORY_CONTROLLER", Name = "Danh mục bài viết", Route = "dashboard/blog-category-manage")]
    public class BlogCategoryController : BaseRoboController<BlogCategory, int, BlogCategoryController, BlogCategoryModel, BlogDbContext>
    {
        private readonly ILogger<BlogCategoryController> logger;
        private readonly IFileUploadService fileUploadService;
        private readonly IBlogCategoryService blogCategoryService;
        private readonly IBlogCategoryMapService blogCategoryMapService;
        private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;

        protected override string IndexFormTitle { get => "Danh mục bài viết"; }

        protected override string CreateFormTitle { get => "Danh mục bài viết - Thêm mới"; }

        protected override string EditFormTitle { get => "Danh mục bài viết - Cập nhật"; }

        protected override string SubmitFormUrl { get; set; }

        protected override string ViewOnlyFormTitle { get => "Danh mục bài viết - Xem chi tiết"; }

        public BlogCategoryController(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IRepository<BlogCategory, BlogDbContext> repository,
            IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo,
            ILogger<BlogCategoryController> logger,
            IMapper mapper,
            IBlogCategoryMapService blogCategoryMapService,
            IBlogCategoryService blogCategoryService,
            FileUploadFactory fileUploadFactory)
            : base(configuration, httpContextAccessor, httpClientFactory, repository, logger, mapper)
        {
            this.logger = logger;
            this.blogCategoryService = blogCategoryService;
            this.blogCategoryMapService = blogCategoryMapService;
            this.blogCategoryMapRepo = blogCategoryMapRepo;
            var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
            this.fileUploadService = fileUploadFactory.GetServiceByName(uploadServiceName);
        }

        protected override void ConvertFromModel(BlogCategoryModel model, BlogCategory entity)
        {
            if (model.Id.HasValue && model.Id.Value != 0)
            {
                entity.Id = model.Id.Value;
            }

            entity.Name = model.Name;
            entity.Slug = !string.IsNullOrWhiteSpace(model.Slug) ? model.Slug : model.Name.ToSlugUrl();
            entity.Enabled = model.Enabled;
            entity.Order = 0;
            entity.Title = model.MetaTitle;
            entity.Keyword = model.MetaKeyword;
            entity.Description = model.MetaDescription;

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

        protected void ConvertFromEntity(BlogCategoryModel model, BlogCategory entity)
        {
            if (model.Id.HasValue && model.Id.Value != 0)
            {
                entity.Id = model.Id.Value;
            }

            BlogCategory parentCategory = null;
            if (entity.ParentId != null)
            {
                parentCategory = blogCategoryService.GetById(entity.ParentId.Value);
            }

            model.Name = entity.Name;
            model.Slug = !string.IsNullOrWhiteSpace(entity.Slug) ? entity.Slug : entity.Name.ToSlugUrl();
            model.Enabled = entity.Enabled;
            model.MetaDescription = entity.Description;
            model.ThumbnailUrl = entity.Thumbnail;
            model.MetaTitle = entity.Title;
            model.MetaKeyword = entity.Keyword;
        }

        protected override BlogCategoryModel GetDefaultModel()
        {
            return new BlogCategoryModel();
        }

        protected override BlogCategory GetDefaultEntity()
        {
            return new BlogCategory();
        }

        public override async Task OnCreatingAsync(RoboUIFormResult<BlogCategoryModel> roboUIForm)
        {
            RoboFormData.RoboFormType = RoboFormType.SubmitIFrame;
            return;
        }

        public override async Task OnEditingAsync(RoboUIFormResult<BlogCategoryModel> roboUIForm, BlogCategory category)
        {
            RoboFormData.RoboFormType = RoboFormType.SubmitIFrame;

            var categories = await blogCategoryService.GetRecordsAsync(s => !s.IsDelete && s.Enabled && s.Id != category.Id);
            var source = categories.ToSelectList(s => s.Id, s => s.Name);
            var selectedItem = source.FirstOrDefault(s => category.ParentId.HasValue && s.Value == category.ParentId.ToString());
            if (selectedItem != null)
            {
                selectedItem.Selected = true;
            }

            return;
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
        public async Task<IActionResult> GetAvailable(string searchTerm)
        {
            var queryDictionary = QueryHelpers.ParseQuery(HttpContext.Request.QueryString.Value);
            var filter = KendoGridFilterParse.Parse(queryDictionary);

            var predicate = LinqExtensions.BuildPredicate(filter, new BlogCategory());
            var source = await blogCategoryService.GetAvailableAsync(searchTerm, predicate, filter.Page, filter.PageSize);

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
            ViewBag.ActiveLink = "BlogCategory";
            base.RoboFormData = new RoboFormModel
            {
                Title = IndexFormTitle,
                RoboFormType = RoboFormType.GirdForm,
                IndexFormSubTitle = "Danh mục bài viết",
                RoboGirdModel = new RoboGirdModel
                {
                    GridHeader = new GridHeader
                    {
                        GridHeaderFields = new List<GridHeaderField>
                        {
                            new GridHeaderField
                            {
                                Label = "Tìm kiếm",
                                PlaceHolder = "Tìm kiếm theo tiêu đề, từ khóa, mô tả",
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
                            FieldName = ObjectExtension.GetRoboFieldName<BlogCategoryResponse>(s => s.Name),
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                        },
                        new RoboGirdField
                        {
                            Title = "HÌNH ẢNH",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogCategoryResponse>(s => s.Thumbnail),
                            RoboGirdFieldType = RoboGirdFieldType.Image,
                        },
                        new RoboGirdField
                        {
                            Title = "SLUG",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogCategoryResponse>(s => s.Slug),
                            RoboGirdFieldType = RoboGirdFieldType.Text
                        },
                        new RoboGirdField
                        {
                            Title = "MÔ TẢ",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogCategoryResponse>(s => s.Description),
                            RoboGirdFieldType = RoboGirdFieldType.Text
                        },
                        new RoboGirdField
                        {
                            Title = "TRẠNG THÁI",
                            FieldName = ObjectExtension.GetRoboFieldName<BlogCategoryResponse>(s => s.Enabled),
                            RoboGirdFieldType = RoboGirdFieldType.Boolean,
                        }
                    },
                    FetchDataUrl = Url.Action("GetAvailable"),
                    CreateButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeMedium,
                        Label = "Thêm mới",
                        Link = "/dashboard/blog-category-manage/create"
                    },
                    ActiveUrl = "/dashboard/blog-category-manage/active",
                    DeleteUrl = "/dashboard/blog-category-manage/delete",
                    EditButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeMedium,
                        Label = "Update",
                        Link = "/dashboard/blog-category-manage/create"
                    }
                }
            };

            var roboForm = await BuildRoboFormAsync(null);
            return roboForm;
        }

        [Route("create/{entityId?}")]
        [PermissionFilter(Code = Permission.Role.Create, Name = Permission.RoleLabel.Create, Route = "create")]
        public override async Task<IActionResult> CreateAsync(int? entityId)
        {
            ViewBag.BlogCategory = "active";

            var model = new BlogCategoryModel();
            dynamic createForm = null;

            var blogs = await blogCategoryService.GetRecordsAsync();
            var blog = blogs.FirstOrDefault(s => s.Id == entityId);

            if (blogs == null)
            {
                blogs = new List<BlogCategory>();
            }

            if (entityId.HasValue)
            {
                ConvertFromEntity(model, blog);

                RoboFormData.Title = EditFormTitle;
                RoboFormData.FormSubmitUrl = SubmitFormUrl;
                RoboFormData.CreateButton = new RoboButton
                {
                    Label = "Lưu"
                };
            }
            else
            {
                RoboFormData.Title = CreateFormTitle;
                RoboFormData.FormSubmitUrl = SubmitFormUrl;
                RoboFormData.CreateButton = new RoboButton
                {
                    Label = "Lưu"
                };
            }

            createForm = await BuildRoboFormAsync(model);
            if (entityId.HasValue && entityId.Value != 0)
            {
                RoboFormData.FormActionType = FormActionType.Edit;
                await OnEditingAsync(createForm, blog);
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
        public override async Task<IActionResult> OnSubmitSaveChangeAsync(int? entityId, BlogCategoryModel model)
        {
            var response = await base.OnSaveChangeAsync(entityId, model);
            await blogCategoryService.SyncBlogCategoryAsync();

            return response;
        }

        [HttpPost, Route("delete")]
        [PermissionFilter(Code = Permission.Role.Delete, Name = Permission.RoleLabel.Delete, Route = "delete")]
        public override async Task<IActionResult> OnDeleteAsync(int id)
        {
            var res = await blogCategoryService.DeleteCategoryAsync(id);

            var blogCategory = await blogCategoryMapService.GetRecordsAsync(s => s.BlogId == id);
            if (blogCategory == null)
            {
                return Json(new BaseResponse<object>
                {
                    Success = true,
                    Message = CommonMessage.RECORD_NOT_FOUND
                });
            }
            else
            {
                foreach (var category in blogCategory)
                {
                    category.IsDelete = true;
                    blogCategoryMapRepo.Update(category);
                }
            }
            await blogCategoryService.SyncBlogCategoryAsync();

            return Json(res);
        }
    }
}