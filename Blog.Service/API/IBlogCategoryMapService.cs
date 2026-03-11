using MicroBase.Repository.Repositories;
using MicroBase.Service;
using Blog.Entity;
using Blog.Entity.Entities;
using Blog.Share.Models;

namespace Blog.Service.API
{
    public interface IBlogCategoryMapService : IGenericService<BlogCategoryMap, BlogDbContext>
    {
        Task<List<BlogCategoryMapResponse>> GetCategoriesByBlogIdAsync(int blogId);
    }

    public class BlogCategoryMapService : GenericService<BlogCategoryMap, BlogDbContext>, IBlogCategoryMapService
    {
        private readonly IRepository<BlogCategory, BlogDbContext> blogCategoryRepo;
        private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;

        public BlogCategoryMapService(IRepository<BlogCategoryMap, BlogDbContext> repository,
            IRepository<BlogCategory, BlogDbContext> blogCategoryRepo,
            IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo)
            : base(repository)
        {
            this.blogCategoryRepo = blogCategoryRepo;
            this.blogCategoryMapRepo = blogCategoryMapRepo;
        }

        public async Task<List<BlogCategoryMapResponse>> GetCategoriesByBlogIdAsync(int blogId)
        {
            var blogs = await blogCategoryMapRepo.FindAsync(s => s.BlogId == blogId && !s.IsDelete && s.Enabled == true);
            var categories = await blogCategoryRepo.FindAsync(s => s.IsDelete == false);
            var blogCategoryMap = new List<BlogCategoryMapResponse>();
            if (blogs != null)
            {
                foreach (var item in blogs)
                {
                    var categoryInfo = categories.FirstOrDefault(s => s.Id == item.BlogCategoryId);
                    blogCategoryMap.Add(new BlogCategoryMapResponse
                    {
                        Code = item.BlogCategoryId,
                        Name = categoryInfo != null ? categoryInfo.Name : string.Empty
                    });
                }
            }

            return blogCategoryMap;
        }
    }
}