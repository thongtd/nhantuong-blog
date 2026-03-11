using System.Linq.Expressions;
using Blog.Entity;
using Blog.Entity.Entities;
using Blog.Service.API;
using Blog.Share.Models;
using MicroBase.Entity;
using MicroBase.RedisProvider;
using MicroBase.Repository.Repositories;
using MicroBase.Service;
using MicroBase.Share.Extensions;
using MicroBase.Share.Linqkit;
using MicroBase.Share.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Blog.Service.CMS
{
    public interface IBlogCMSService : IGenericService<Entity.Entities.Blog, BlogDbContext>
    {
        Task<TPaging<BlogDetailsResponse>> GetAvailableAsync(string name, Expression<Func<Entity.Entities.Blog, bool>> expression,
            int pageIndex,
            int pageSize);

        Task<object> GetTextInFormAsync(int blogId);
    }

    public class BlogCMSService : GenericService<Entity.Entities.Blog, BlogDbContext>, IBlogCMSService
    {
        private readonly IBlogCategoryService blogCategoryService;
        private readonly IConfiguration configuration;
        private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;
        private readonly IRepository<Entity.Entities.Blog, BlogDbContext> blogRepo;
        public readonly BlogDbContext blogDbContext;
        public readonly MicroDbContext microDbContext;

        public BlogCMSService(IRepository<Entity.Entities.Blog, BlogDbContext> repository,
            IBlogCategoryService blogCategoryService,
            IConfiguration configuration,
            IRedisStogare redisStogare,
            ILogger<BlogCMSService> logger,
            IRepository<Entity.Entities.Blog, BlogDbContext> blogRepo,
            IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo,
            BlogDbContext blogDbContext,
            MicroDbContext microDbContext)
            : base(repository)
        {
            var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
            this.blogCategoryMapRepo = blogCategoryMapRepo;
            this.blogRepo = blogRepo;
            this.blogCategoryService = blogCategoryService;
            this.configuration = configuration;
            this.blogDbContext = blogDbContext;
            this.microDbContext = microDbContext;
        }

        public async Task<TPaging<BlogDetailsResponse>> GetAvailableAsync(string name,
            Expression<Func<Entity.Entities.Blog, bool>> expression,
            int pageIndex,
            int pageSize)
        {
            int index = (pageIndex - 1) * pageSize;
            int totalRows = 0;
            totalRows = (int)await CountAsync(expression);

            var predicate = PredicateBuilder.Create<Entity.Entities.Blog>(s => !s.IsDelete);
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.CustomTrim().ToUpper();
                predicate = predicate.And(s => s.Name.ToUpper().Contains(name) || s.Slug.ToUpper().Contains(name));
            }

            totalRows = await blogDbContext.Blogs
                .Where(predicate)
                .CountAsync();

            var blogs = await blogDbContext.Blogs
                .Where(predicate)
                .OrderByDescending(s => s.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var blogCategoryMap = await blogCategoryMapRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);
            var blogCategory = await blogCategoryService.GetRecordsAsync(s => s.IsDelete == false && s.Enabled == true);

            var blogDetails = new List<BlogDetailsResponse>();
            foreach (var blog in blogs)
            {
                string categoryName = "";
                var category = blogCategoryMap.Where(s => s.BlogId == blog.Id).FirstOrDefault();
                if (category != null)
                {
                    categoryName = blogCategory.FirstOrDefault(s => s.Id == category.BlogCategoryId)?.Name ?? "";
                }

                blogDetails.Add(new BlogDetailsResponse
                {
                    Order = ++index,
                    Id = blog.Id,
                    Name = blog.Name,
                    Slug = blog.Slug,
                    Thumbnail = blog.Thumbnail,
                    View = blog.View,
                    Enabled = blog.Enabled,
                    HotNews = blog.HotNews,
                    CreatedDate = blog.CreatedDate,
                    CategoryName = categoryName
                });
            }

            return new TPaging<BlogDetailsResponse>
            {
                Source = blogDetails,
                TotalRecords = totalRows
            };
        }

        public async Task<object> GetTextInFormAsync(int blogId)
        {
            var blogEditResonse = new BlogEditResponse();
            var blog = await blogRepo.FindOneAsync(s => s.Id == blogId && !s.IsDelete);

            if (blog != null)
            {
                blogEditResonse.Name = blog.Name;
                blogEditResonse.Title = blog.Title;
            }

            return blogEditResonse;
        }
    }
}
