using AutoMapper;
using MicroBase.Share.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroBase.Dashboard.Robo.Controllers.RoboForm;
using MicroBase.Dashboard.Robo.Attributes;
using MicroBase.FileManager;
using MicroBase.Dashboard.Share.Models.RoboForm.UI;
using MicroBase.Dashboard.Share.Models;
using MicroBase.Dashboard.Robo.Code;
using MicroBase.Dashboard.Robo.Filters;
using MicroBase.Repository.Repositories;
using Blog.Entity.Entities;
using Blog.Entity;
using Blog.Share.Models.CMS;
using Blog.Share.Models;
using Blog.Service.API;

namespace Blog.Dashboard.Controllers
{
    [Route("dashboard/blog-tag-manage")]
    [DashboardActionFilter(IndexPageTile = "Tag bài viết", EditPageTile = "Cập nhật", CreatePageTile = "Thêm mới")]
    [PermissionFilter(Code = "BLOG_TAG_CONTROLLER", Name = "Tag bài viết", Route = "dashboard/blog-tag-manage")]
    public class BlogTagController : BaseRoboController<BlogTag, int, BlogTagController, TagModel, BlogDbContext>
    {
        private readonly ILogger<BlogTagController> logger;
        private readonly IFileUploadService fileUploadService;
        private readonly ITagService tagService;

        protected override string IndexFormTitle { get => "Tag bài viết"; }

        protected override string CreateFormTitle { get => "Tag bài viết - Thêm mới"; }

        protected override string EditFormTitle { get => "Tag bài viết - Cập nhật"; }

        protected override string SubmitFormUrl { get; set; }

        protected override string ViewOnlyFormTitle { get => "Tag bài viết - Xem chi tiết"; }

        public BlogTagController(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IRepository<BlogTag, BlogDbContext> repository,
            ILogger<BlogTagController> logger,
            IMapper mapper,
            ITagService tagService,
            FileUploadFactory fileUploadFactory)
            : base(configuration, httpContextAccessor, httpClientFactory, repository, logger, mapper)
        {
            this.logger = logger;
            var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
            this.fileUploadService = fileUploadFactory.GetServiceByName(uploadServiceName);
            this.tagService = tagService;
        }

        protected override void ConvertFromModel(TagModel model, BlogTag entity)
        {
            if (model.Id.HasValue && model.Id.Value != 0)
            {
                entity.Id = model.Id.Value;
            }

            entity.Keyword = model.Keyword;
            entity.Enabled = model.Enabled;
            entity.Slug = "";
            entity.Description = model.Description;
            entity.NomalizationKeyword = model.Keyword.ToLower();
            entity.Order = 0;
        }

        protected void ConvertFromEntity(TagModel model, BlogTag entity)
        {
            if (model.Id.HasValue && model.Id.Value != 0)
            {
                entity.Id = model.Id.Value;
            }

            model.Keyword = entity.Keyword;
            model.Enabled = entity.Enabled;
            model.Description = entity.Description;
        }

        protected override TagModel GetDefaultModel()
        {
            return new TagModel();
        }
        protected override BlogTag GetDefaultEntity()
        {
            return new BlogTag();
        }

        [Route("create/{entityId?}")]
        [PermissionFilter(Code = Permission.Role.Create, Name = Permission.RoleLabel.Create, Route = "create")]
        public override async Task<IActionResult> CreateAsync(int? entityId)
        {
            ViewBag.BlogTag = "active";

            var model = new TagModel();
            dynamic createForm = null;

            var tags = await tagService.GetRecordsAsync();
            var tag = tags.FirstOrDefault(s => s.Id == entityId);

            if (tags == null)
            {
                tags = new List<BlogTag>();
            }

            if (entityId.HasValue)
            {
                ConvertFromEntity(model, tag);

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
                await OnEditingAsync(createForm, tag);
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
        public override async Task<IActionResult> OnSubmitSaveChangeAsync(int? entityId, TagModel model)
        {
            var response = await base.OnSaveChangeAsync(entityId, model);
            await tagService.SyncTagToCacheAsync();

            return response;
        }

        [HttpPost, Route("delete")]
        [PermissionFilter(Code = Permission.Role.Delete, Name = Permission.RoleLabel.Delete, Route = "delete")]
        public override async Task<IActionResult> OnDeleteAsync(int id)
        {
            var response = await base.DeleteAsync(id);
            await tagService.SyncTagToCacheAsync();

            return response;
        }

        [HttpGet, Route("get-available")]
        [PermissionFilter(Code = Permission.Role.View, Name = Permission.RoleLabel.View, Route = "get-available")]
        public async Task<IActionResult> GetAvailable(string searchTerm)
        {
            var queryDictionary = QueryHelpers.ParseQuery(HttpContext.Request.QueryString.Value);
            var filter = KendoGridFilterParse.Parse(queryDictionary);

            var source = await tagService.GetTagsAsync(searchTerm, filter.Page, filter.PageSize);

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
            ViewBag.Tag = "active";
            ViewBag.ActiveLink = "BlogTag";
            base.RoboFormData = new RoboFormModel
            {
                Title = IndexFormTitle,
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
                                PlaceHolder = "Tìm kiếm theo từ khóa, mô tả",
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
                            Title = "TỪ KHÓA",
                            FieldName = ObjectExtension.GetRoboFieldName<TagsResponse>(s => s.Keyword),
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                        },
                        new RoboGirdField
                        {
                            Title = "MÔ TẢ",
                            FieldName = ObjectExtension.GetRoboFieldName<TagsResponse>(s => s.Description),
                            RoboGirdFieldType = RoboGirdFieldType.Text,
                        },
                        new RoboGirdField
                        {
                            Title = "TRẠNG THÁI",
                            FieldName = ObjectExtension.GetRoboFieldName<TagsResponse>(s => s.Enabled),
                            RoboGirdFieldType = RoboGirdFieldType.Boolean,
                        }
                    },
                    FetchDataUrl = Url.Action("GetAvailable"),
                    CreateButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeMedium,
                        Label = "Thêm mới",
                        Link = "/dashboard/blog-tag-manage/create"
                    },
                    ActiveUrl = "/dashboard/blog-tag-manage/active",
                    DeleteUrl = "/dashboard/blog-tag-manage/delete",
                    EditButton = new RoboButton
                    {
                        Action = ButtonAction.OpenIframeMedium,
                        Label = "Update",
                        Link = "/dashboard/blog-tag-manage/create"
                    }
                }
            };

            var roboForm = await BuildRoboFormAsync(null);
            return roboForm;
        }
    }
}