using Blog.Entity;
using Blog.Entity.Entities;
using Blog.Share.Models;
using MicroBase.RedisProvider;
using MicroBase.Repository.Repositories;
using MicroBase.Share.Linqkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Service.API
{
    public interface IBlogCacheService
    {
        Task BuildGroupsToCacheAsync();

        Task<IEnumerable<GroupModel>> GetGroupInfoAsync(string slug);

        Task BuildTagsToCacheAsync();

        Task<IEnumerable<GroupModel>> GetTagInfoAsync(string slug);

        Task BuildBlogsToCacheAsync(int? id = null);

        Task<IEnumerable<BlogDetailsResponse>> GetBlogInfoAsync(string slug);
    }

    public class BlogCacheService : IBlogCacheService
    {
        private readonly ILogger<BlogCacheService> logger;
        private readonly IRedisStogare redisStogare;
        private readonly IRepository<BlogCategory, BlogDbContext> blogCategoryRepo;
        private readonly IRepository<BlogTag, BlogDbContext> blogTagRepo;
        private readonly BlogDbContext blogDbContext;

        private readonly string BLOG_CATEGORY = "BLOG_CATEGORY";
        private readonly string BLOG_TAG = "BLOG_TAG";
        private readonly string BLOG = "BLOG";

        public BlogCacheService(ILogger<BlogCacheService> logger,
            IRedisStogare redisStogare,
            IRepository<BlogCategory, BlogDbContext> blogCategoryRepo,
            IRepository<BlogTag, BlogDbContext> blogTagRepo,
            BlogDbContext blogDbContext)
        {
            this.logger = logger;
            this.blogTagRepo = blogTagRepo;
            this.blogCategoryRepo = blogCategoryRepo;
            this.blogDbContext = blogDbContext;
            this.redisStogare = redisStogare;
        }

        public async Task BuildGroupsToCacheAsync()
        {
            var records = await blogCategoryRepo.FindAsync(s => s.IsDelete == false);
            var caches = new Dictionary<string, GroupModel>();
            foreach (var s in records)
            {
                if (!caches.ContainsKey(s.Slug))
                {
                    caches.Add(s.Slug, new GroupModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Slug = s.Slug,
                        Title = s.Title,
                        Description = s.Description,
                        Thumbnail = s.Thumbnail
                    });
                }
            }

            await redisStogare.HSetAsync(BLOG_CATEGORY, caches);
        }

        public async Task BuildTagsToCacheAsync()
        {
            var records = await blogTagRepo.FindAsync(s => s.IsDelete == false);
            var caches = new Dictionary<string, GroupModel>();
            foreach (var s in records)
            {
                if (!caches.ContainsKey(s.Slug))
                {
                    caches.Add(s.Slug, new GroupModel
                    {
                        Id = s.Id,
                        Name = s.NomalizationKeyword,
                        Slug = s.Slug,
                        Title = s.Keyword,
                        Description = s.Description
                    });
                }
            }

            await redisStogare.HSetAsync(BLOG_TAG, caches);
        }

        public async Task BuildBlogsToCacheAsync(int? id = null)
        {
            var predicate = PredicateBuilder.Create<Entity.Entities.Blog>(s => s.IsDelete == false);
            if (id.HasValue)
            {
                predicate = predicate.And(s => s.Id == id.Value);
            }

            var records = await blogDbContext.Set<Entity.Entities.Blog>()
                .Include(s => s.BlogCategoryMaps)
                .ThenInclude(s => s.BlogCategory)
                .Include(s => s.BlogTagMaps)
                .ThenInclude(s => s.BlogTag)
                .Where(predicate)
                .ToListAsync();

            var caches = new Dictionary<string, BlogDetailsResponse>();
            foreach (var s in records)
            {
                if (!caches.ContainsKey(s.Slug))
                {
                    caches.Add(s.Slug, new BlogDetailsResponse
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Title = s.Title,
                        BodyContent = s.BodyContent,
                        Description = s.Description,
                        Keyword = s.Keyword,
                        CreatedDate = s.CreatedDate,
                        HotNews = s.HotNews,
                        Slug = s.Slug,
                        SubContent = s.SubContent,
                        Thumbnail = s.Thumbnail,
                        BlogCategory = s.BlogCategoryMaps.Select(c => new BlogCategoryRes
                        {
                            Id = c.BlogCategoryId,
                            Name = c.BlogCategory.Name,
                            Slug = c.BlogCategory.Slug
                        }).ToList(),
                        BlogTag = s.BlogTagMaps.Select(t => new BlogTagRes
                        {
                            Id = t.TagId,
                            Keyword = t.BlogTag.Keyword,
                            Slug = t.BlogTag.Slug,
                            NomalizationKeyword = t.BlogTag.NomalizationKeyword
                        }).ToList()
                    });
                }
            }

            await redisStogare.HSetAsync(BLOG, caches);
        }

        public async Task<IEnumerable<GroupModel>> GetGroupInfoAsync(string slug)
        {
            if (!string.IsNullOrEmpty(slug))
            {
                var record = await redisStogare.HGetAsync<GroupModel>(BLOG_CATEGORY, slug);
                return new List<GroupModel> { record };
            }

            var records = await redisStogare.HGetAllAsync<GroupModel>(BLOG_CATEGORY);
            return records;
        }

        public async Task<IEnumerable<GroupModel>> GetTagInfoAsync(string slug)
        {
            if (!string.IsNullOrEmpty(slug))
            {
                var record = await redisStogare.HGetAsync<GroupModel>(BLOG_TAG, slug);
                return new List<GroupModel> { record };
            }

            var records = await redisStogare.HGetAllAsync<GroupModel>(BLOG_TAG);
            return records;
        }

        public async Task<IEnumerable<BlogDetailsResponse>> GetBlogInfoAsync(string slug)
        {
            if (!string.IsNullOrEmpty(slug))
            {
                var record = await redisStogare.HGetAsync<BlogDetailsResponse>(BLOG, slug);
                return new List<BlogDetailsResponse> { record };
            }

            var records = await redisStogare.HGetAllAsync<BlogDetailsResponse>(BLOG);
            return records;
        }
    }
}