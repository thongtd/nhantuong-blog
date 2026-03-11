using Microsoft.EntityFrameworkCore;
using MicroBase.Share;
using MicroBase.Share.Extensions;
using Microsoft.Extensions.Configuration;
using MicroBase.RedisProvider;
using Microsoft.Extensions.Logging;
using MicroBase.Repository.Repositories;
using MicroBase.Share.Models.Base;
using Microsoft.AspNetCore.Http;
using Blog.Entity;
using Blog.Share.Models;
using Blog.Entity.Entities;
using Blog.Share;
using MicroBase.Entity;
using Pipelines.Sockets.Unofficial.Arenas;
using MicroBase.Repository.Models.SqlModels;
using MicroBase.Service.SqlServices;
using System.Data;
using MySqlConnector;
using System.Linq;

namespace Blog.Service.API
{
    public interface IBlogService
    {
        Task<TPaging<BlogResponse>> GetBlogsAsync(
            string? name,
            string? slug,
            string? content,
            int? categoryId,
            string? categorySlug,
            int? tagId,
            int pageIndex,
            int pageSize);

        Task<BaseResponse<BlogResponse>> GetBlogById(int? id, string? slug);

        Task<IEnumerable<TopBlogModel>> GetTopBlogAsync();

        Task<GroupBlogModel> GetBlogsAsync(string blogCategorySlug, string tagSlug, int pageIndex, int pageSize);
    }

    public class BlogService : IBlogService
    {
        private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;
        private readonly IRepository<BlogCategory, BlogDbContext> blogCategoryRepo;
        private readonly IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo;
        private readonly IRepository<BlogTag, BlogDbContext> blogTagRepo;
        private readonly IRedisStogare redisStogare;
        private readonly ILogger<BlogService> logger;
        public readonly BlogDbContext blogDbContext;
        public readonly IConfiguration configuration;
        public readonly MicroDbContext microDbContext;
        private readonly ISqlExecuteService sqlExecuteService;
        private readonly IBlogCacheService blogCacheService;

        public BlogService(IRepository<Entity.Entities.Blog, BlogDbContext> repository,
            IConfiguration configuration,
            IRedisStogare redisStogare,
            ILogger<BlogService> logger,
            IRepository<Entity.Entities.Blog, BlogDbContext> blogRepo,
            IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo,
            IRepository<BlogTag, BlogDbContext> blogTagRepo,
            IRepository<BlogCategory, BlogDbContext> blogCategoryRepo,
            IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo,
            BlogDbContext blogDbContext,
            MicroDbContext microDbContext,
            ISqlExecuteService sqlExecuteService,
            IBlogCacheService blogCacheService)
        {
            var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");

            this.blogCategoryMapRepo = blogCategoryMapRepo;
            this.blogTagMapRepo = blogTagMapRepo;
            this.blogTagRepo = blogTagRepo;
            this.configuration = configuration;
            this.redisStogare = redisStogare;
            this.blogCategoryRepo = blogCategoryRepo;
            this.logger = logger;
            this.blogDbContext = blogDbContext;
            this.microDbContext = microDbContext;
            this.sqlExecuteService = sqlExecuteService;
            this.blogCacheService = blogCacheService;
        }

        public async Task<TPaging<BlogResponse>> GetBlogsAsync(string? name,
            string? slug,
            string? content,
            int? categoryId,
            string? categorySlug,
            int? tagId,
            int pageIndex,
            int pageSize)
        {
            try
            {
                //var cacheKeyBlog = BlogConstant.RedisKey.CACHE_KEY_BLOG;

                //// Cache BlogCategoryMap
                //var cacheKeyBlogCategoryMap = BlogConstant.RedisKey.CACHE_KEY_BLOG_CATEGORY_MAP;
                //var cachedBlogCategoryMapEntities = await redisStogare.GetAsync<List<BlogCategoryMap>>(cacheKeyBlogCategoryMap)
                //    ?? await GetBlogCategoryMapCacheAsync();

                //await redisStogare.SetAsync(cacheKeyBlogCategoryMap, cachedBlogCategoryMapEntities);

                //// Cache BlogCategory
                //var cacheKeyBlogCategory = BlogConstant.RedisKey.CACHE_KEY_BLOG_CATEGORY;
                //var cachedBlogCategoryEntities = await redisStogare.GetAsync<List<BlogCategory>>(cacheKeyBlogCategory)
                //    ?? await GetBlogCategoryCacheAsync();

                //await redisStogare.SetAsync(cacheKeyBlogCategory, cachedBlogCategoryEntities);

                //// Cache BlogTagMap
                //var cacheKeyBlogTagMap = BlogConstant.RedisKey.CACHE_KEY_BLOG_TAG_MAP;
                //var cachedBlogTagMapEntities = await redisStogare.GetAsync<List<BlogTagMap>>(cacheKeyBlogTagMap)
                //    ?? await GetBlogTagMapCacheAsync();

                //await redisStogare.SetAsync(cacheKeyBlogTagMap, cachedBlogTagMapEntities);

                //// Cache BlogTag
                //var cacheKeyBlogTag = BlogConstant.RedisKey.CACHE_KEY_BLOG_TAG;
                //var cachedBlogTagEntities = await redisStogare.GetAsync<List<BlogTag>>(cacheKeyBlogTag)
                //    ?? await GetBlogTagCacheAsync();

                //await redisStogare.SetAsync(cacheKeyBlogTag, cachedBlogTagEntities);

                //// Lọc theo categoryId, tagId
                //var filteredBlogIdsByCategory = categoryId.HasValue
                //    ? cachedBlogCategoryMapEntities
                //        .Where(x => x.BlogCategoryId == categoryId.Value)
                //        .Select(x => x.BlogId)
                //        .Distinct()
                //        .ToList()
                //    : null;

                //var filteredBlogIdsByCategorySlug = !string.IsNullOrEmpty(categorySlug)
                //    ? cachedBlogCategoryMapEntities
                //        .Where(x => x.BlogCategory.Slug == categorySlug)
                //        .Select(x => x.BlogId)
                //        .Distinct()
                //        .ToList()
                //    : null;

                //var filteredBlogIdsByTag = tagId.HasValue
                //    ? cachedBlogTagMapEntities
                //        .Where(x => x.TagId == tagId.Value)
                //        .Select(x => x.BlogId)
                //        .Distinct()
                //        .ToList()
                //    : null;

                //var blogQuery = blogDbContext.Blogs.Where(x => x.Enabled == true).AsQueryable();

                //if (filteredBlogIdsByCategory != null)
                //{
                //    blogQuery = blogQuery.Where(b => filteredBlogIdsByCategory.Contains(b.Id));
                //}

                //if (filteredBlogIdsByCategorySlug != null)
                //{
                //    blogQuery = blogQuery.Where(b => filteredBlogIdsByCategorySlug.Contains(b.Id));
                //}

                //if (filteredBlogIdsByTag != null)
                //{
                //    blogQuery = blogQuery.Where(b => filteredBlogIdsByTag.Contains(b.Id));
                //}

                //var blogs = blogQuery
                //    .OrderByDescending(s => s.CreatedDate)
                //    .Skip((pageIndex - 1) * pageSize)
                //    .Take(pageSize)
                //    .ToList();

                //// Lấy tác giả
                //var createdByIds = blogs.Select(x => x.CreatedBy).Distinct().ToList();
                //var authors = microDbContext.Users
                //    .Where(u => createdByIds.Contains(u.Id))
                //    .ToDictionary(u => u.Id, u => u.FullName);

                //var blogsResponse = new List<BlogResponse>();

                //foreach (var blog in blogs)
                //{
                //    var categories = cachedBlogCategoryMapEntities
                //        .Where(s => s.BlogId == blog.Id)
                //        .ToList();

                //    var categoryInfo = new List<BlogCategoryRes>();
                //    foreach (var categoryItem in categories)
                //    {
                //        var category = cachedBlogCategoryEntities
                //            .FirstOrDefault(s => s.Id == categoryItem.BlogCategoryId);
                //        if (category != null)
                //        {
                //            categoryInfo.Add(new BlogCategoryRes
                //            {
                //                Id = category.Id,
                //                Name = category.Name,
                //                Slug = category.Slug,
                //            });
                //        }
                //    }

                //    var tags = cachedBlogTagMapEntities
                //        .Where(s => s.BlogId == blog.Id)
                //        .ToList();

                //    var tagInfo = new List<BlogTagRes>();
                //    foreach (var tagItem in tags)
                //    {
                //        var tag = cachedBlogTagEntities
                //            .FirstOrDefault(s => s.Id == tagItem.TagId);
                //        if (tag != null)
                //        {
                //            tagInfo.Add(new BlogTagRes
                //            {
                //                Id = tag.Id,
                //                Keyword = tag.Keyword,
                //                Slug = tag.Slug
                //            });
                //        }
                //    }

                //    blogsResponse.Add(new BlogResponse
                //    {
                //        Id = blog.Id,
                //        Name = blog.Name,
                //        View = blog.View,
                //        Author = authors.ContainsKey(blog.CreatedBy.Value) ? authors[blog.CreatedBy.Value] : null,
                //        CreatedDate = blog.CreatedDate,
                //        BlogCategory = categoryInfo,
                //        BlogTag = tagInfo,
                //        Slug = blog.Slug,
                //        Thumbnail = blog.Thumbnail,
                //        BodyContent = blog.BodyContent,
                //        SubContent = blog.SubContent
                //    });
                //}

                //if (!string.IsNullOrWhiteSpace(name))
                //{
                //    blogsResponse = blogsResponse
                //        .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                //        .ToList();
                //}

                //if (!string.IsNullOrWhiteSpace(slug))
                //{
                //    blogsResponse = blogsResponse
                //        .Where(x => x.Slug.Contains(slug, StringComparison.OrdinalIgnoreCase))
                //        .ToList();
                //}

                //if (!string.IsNullOrWhiteSpace(content))
                //{
                //    blogsResponse = blogsResponse
                //        .Where(x => x.BodyContent.Contains(content, StringComparison.OrdinalIgnoreCase))
                //        .ToList();
                //}

                var blogs = await blogCacheService.GetBlogInfoAsync(slug: slug);
                blogs = blogs.OrderByDescending(s => s.CreatedDate)
                    .Skip((pageIndex - 1) * pageIndex)
                    .Take(pageSize);

                return new TPaging<BlogResponse>
                {
                    Source = blogs.Select(s => new BlogResponse
                    {
                        Id = s.Id,
                        BodyContent = s.BodyContent,
                        CreatedDate = s.CreatedDate,
                        Name = s.Name,
                        Slug = s.Slug,
                        SubContent = s.SubContent,
                        Thumbnail = s.Thumbnail,
                        BlogTag = s.BlogTag,
                        BlogCategory = s.BlogCategory
                    }),
                    TotalRecords = blogs.Count()
                };
            }
            catch (Exception ex)
            {
                var dataLog = new
                {
                    Name = name,
                    Slug = slug,
                    Content = content,
                    CategoryId = categoryId,
                    tagId
                };
                logger.LogError($"\n\tGetBlogsAsync at IBlogService" +
                    $"\n\tData: {dataLog}" +
                    $"\n\tError: {ex.Message}");
                return new TPaging<BlogResponse>();
            }
        }

        public async Task<BaseResponse<BlogResponse>> GetBlogById(int? id, string? slug)
        {
            try
            {
                var blogs = await blogCacheService.GetBlogInfoAsync(slug);
                var blogResponse = new BlogResponse();
                if (blogs.Any())
                {
                    var s = blogs.FirstOrDefault();
                    blogResponse = new BlogResponse
                    {
                        Id = s.Id,
                        BodyContent = s.BodyContent,
                        CreatedDate = s.CreatedDate,
                        Name = s.Name,
                        Slug = s.Slug,
                        SubContent = s.SubContent,
                        Thumbnail = s.Thumbnail,
                        BlogTag = s.BlogTag,
                        BlogCategory = s.BlogCategory,
                        Title = s.Title,
                        Description = s.Description
                    };
                }

                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status200OK,
                    Data = blogResponse
                };
            }
            catch (Exception ex)
            {
                var dataLog = new
                {
                    Id = id,
                    Slug = slug
                };

                logger.LogError($"\n\tGetBlogById at IBlogService" +
                    $"\n\tData: {dataLog}" +
                    $"\n\tError: {ex.Message}");

                return new BaseResponse<BlogResponse>
                {
                    Message = CommonMessage.UN_DETECTED_ERROR,
                    Code = StatusCodes.Status400BadRequest
                };
            }
        }

        public async Task<IEnumerable<TopBlogModel>> GetTopBlogAsync()
        {
            try
            {
                var sqlParams = new List<SqlParamModel>();
                var records = await sqlExecuteService.ExecuteProcReturnAsync<TopBlogSqlModel>(funcName: "proc_get_top_blogs", sqlParams);
                if (!records.Any())
                {
                    return Enumerable.Empty<TopBlogModel>();
                }

                var blogs = new List<TopBlogModel>();
                foreach (var gr in records.GroupBy(s => s.BlogCategoryId))
                {
                    var record = records.FirstOrDefault(s => s.BlogCategoryId == gr.Key);
                    if (record == null)
                    {
                        continue;
                    }

                    blogs.Add(new TopBlogModel
                    {
                        BlogCategoryId = record.BlogCategoryId,
                        CategoryName = record.CategoryName,
                        CategorySlug = record.CategorySlug,
                        Blogs = gr.Select(s => new BlogResponse
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Slug = s.Slug,
                            Thumbnail = s.Thumbnail,
                            SubContent = s.SubContent
                        }).ToList()
                    });
                }

                return blogs;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return Enumerable.Empty<TopBlogModel>();
            }
        }

        private async Task<List<BlogCategoryMap>> GetBlogCategoryMapCacheAsync()
        {
            return (List<BlogCategoryMap>)await blogCategoryMapRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);
        }

        private async Task<List<BlogCategory>> GetBlogCategoryCacheAsync()
        {
            return (List<BlogCategory>)await blogCategoryRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);
        }

        private async Task<List<BlogTagMap>> GetBlogTagMapCacheAsync()
        {
            return (List<BlogTagMap>)await blogTagMapRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);
        }

        private async Task<List<BlogTag>> GetBlogTagCacheAsync()
        {
            return (List<BlogTag>)await blogTagRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);
        }

        public async Task<GroupBlogModel> GetBlogsAsync(string blogCategorySlug, string tagSlug, int pageIndex, int pageSize)
        {
            try
            {
                var sqlParams = new List<SqlParamModel>
                {
                    new SqlParamModel("p_category_slug", blogCategorySlug, ParameterDirection.Input, MySqlDbType.VarChar),
                    new SqlParamModel("p_tag_slug", tagSlug, ParameterDirection.Input, MySqlDbType.VarChar),
                    new SqlParamModel("p_page_index", pageIndex, ParameterDirection.Input, MySqlDbType.Int64),
                    new SqlParamModel("p_page_size", pageSize, ParameterDirection.Input, MySqlDbType.Int64),
                };

                var records = await sqlExecuteService.ExecuteProcReturnPagingAsync<TopBlogSqlModel>(funcName: "proc_get_blogs", sqlParams);
                var group = new GroupModel();
                if (!string.IsNullOrEmpty(blogCategorySlug))
                {
                    var blogCategories = await blogCacheService.GetGroupInfoAsync(blogCategorySlug);
                    if (blogCategories != null && blogCategories.Any())
                    {
                        group = blogCategories.FirstOrDefault();
                    }
                }
                else if (!string.IsNullOrEmpty(tagSlug))
                {
                    var blogTags = await blogCacheService.GetTagInfoAsync(tagSlug);
                    if (blogTags != null && blogTags.Any())
                    {
                        group = blogTags.FirstOrDefault();
                    }
                }

                var res = new GroupBlogModel
                {
                    Group = group,
                    Blogs = records
                };

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new GroupBlogModel();
            }
        }
    }
}