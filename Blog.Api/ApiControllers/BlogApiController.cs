using Microsoft.AspNetCore.Mvc;
using MicroBase.Share.Models.Base;
using Blog.Share.Models;
using Blog.Service.API;

namespace Blog.Api.ApiControllers
{
    [ApiController]
    [Route("api-v1/blogs")]
    public class BlogApiController : ControllerBase
    {
        private readonly IBlogService blogService;
        private readonly IBlogCategoryService blogCategoryService;
        private readonly IBlogCacheService blogCacheService;

        public BlogApiController(IBlogService blogService,
            IBlogCategoryService blogCategoryService,
            IBlogCacheService blogCacheService)
        {
            this.blogService = blogService;
            this.blogCategoryService = blogCategoryService;
            this.blogCacheService = blogCacheService;
        }

        [HttpGet("get-all")]
        public async Task<BaseResponse<TPaging<BlogResponse>>> GetBlogs(
            int? categoryId,
            string? categorySlug,
            int? tagId,
            string? name,
            string? slug,
            string? content,
            int pageIndex = 1,
            int pageSize = 8)
        {
            var blogs = await blogService.GetBlogsAsync(name, slug, content, categoryId, categorySlug, tagId, pageIndex, pageSize);
            return new BaseResponse<TPaging<BlogResponse>>
            {
                Success = true,
                Data = blogs
            };
        }

        [HttpGet("get-by-id")]
        public async Task<BaseResponse<BlogResponse>> GetBlogById(int? id, string? slug)
        {
            return await blogService.GetBlogById(id, slug);
        }

        [HttpGet("get-category")]
        public async Task<BaseResponse<TPaging<BlogCategoryApiResponse>>> GetCategories(string? name)
        {
            var blogs = await blogCategoryService.GetCategoriesAsync(name);
            return new BaseResponse<TPaging<BlogCategoryApiResponse>>
            {
                Success = true,
                Data = blogs
            };
        }

        [HttpGet("get-top-blogs")]
        public async Task<BaseResponse<IEnumerable<TopBlogModel>>> GetTopBlogs()
        {
            var blogs = await blogService.GetTopBlogAsync();
            return new BaseResponse<IEnumerable<TopBlogModel>>
            {
                Success = true,
                Data = blogs
            };
        }

        [HttpGet("get-blogs")]
        public async Task<BaseResponse<GroupBlogModel>> GetBlogs(string? blogCategorySlug, string? tagSlug, int pageIndex = 1, int pageSize = 10)
        {
            var blogs = await blogService.GetBlogsAsync(blogCategorySlug, tagSlug, pageIndex, pageSize);
            return new BaseResponse<GroupBlogModel>
            {
                Success = true,
                Data = blogs
            };
        }

        [HttpGet("build-cache")]
        public async Task<BaseResponse<object>> BuildToCache()
        {
            await blogCacheService.BuildGroupsToCacheAsync();
            await blogCacheService.BuildBlogsToCacheAsync();
            await blogCacheService.BuildTagsToCacheAsync();
            return new BaseResponse<object>
            {
                Success = true
            };
        }
    }
}