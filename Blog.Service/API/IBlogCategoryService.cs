using MicroBase.Share.Extensions;
using MicroBase.Share;
using System.Linq.Expressions;
using System.Transactions;
using MicroBase.RedisProvider;
using MicroBase.Repository.Repositories;
using MicroBase.Share.Models.Base;
using Microsoft.AspNetCore.Http;
using MicroBase.Service;
using Blog.Entity.Entities;
using Blog.Entity;
using Blog.Share.Models;
using Blog.Share;

namespace Blog.Service.API
{
    public interface IBlogCategoryService : IGenericService<BlogCategory, BlogDbContext>
    {
        Task<TPaging<BlogCategoryResponse>> GetAvailableAsync(string searchTerm, Expression<Func<BlogCategory, bool>> expression,
            int pageIndex,
            int pageSize);

        Task<BaseResponse<object>> AddBlogCategoryAsync(BlogCategoryRequest model);

        Task<BaseResponse<object>> UpdateBlogCategoryAsync(int id, BlogCategoryRequest model);

        Task<TPaging<BlogCategoryApiResponse>> GetCategoriesAsync(string? name);

        Task<BaseResponse<object>> GetBlogCategoriesById(int blogId);

        Task<BaseResponse<object>> DeleteCategoryAsync(int entityId);

        Task<BaseResponse<bool>> SyncBlogCategoryAsync();
    }

    public class BlogCategoryService : GenericService<BlogCategory, BlogDbContext>, IBlogCategoryService
    {
        private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;
        private readonly IRedisStogare redisStogare;
        private readonly IRepository<BlogCategory, BlogDbContext> blogCategoryRepo;

        public BlogCategoryService(IRepository<BlogCategory, BlogDbContext> repository,
            IRedisStogare redisStogare,
            IRepository<BlogCategory, BlogDbContext> blogCategoryRepo,
            IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo) : base(repository)
        {
            this.blogCategoryMapRepo = blogCategoryMapRepo;
            this.redisStogare = redisStogare;
            this.blogCategoryRepo = blogCategoryRepo;
        }

        public async Task<BaseResponse<bool>> SyncBlogCategoryAsync()
        {
            try
            {
                var cacheKeyBlogCategory = BlogConstant.RedisKey.CACHE_KEY_BLOG_CATEGORY;
                var cachedBlogCategoryEntities = await GetBlogCategoryCacheAsync();

                await redisStogare.SetAsync(cacheKeyBlogCategory, cachedBlogCategoryEntities);

                return new BaseResponse<bool>
                {
                    Success = true,
                    Code = StatusCodes.Status200OK,
                    Message = BlogConstant.CommonMessageBlog.SYNC_BLOG_CATEGORY_SUCCESS,
                    MessageCode = nameof(BlogConstant.CommonMessageBlog.SYNC_BLOG_CATEGORY_SUCCESS)
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Success = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = CommonMessage.UN_DETECTED_ERROR,
                    MessageCode = nameof(CommonMessage.UN_DETECTED_ERROR)
                };
            }

        }

        private async Task<List<BlogCategory>> GetBlogCategoryCacheAsync()
        {
            return (List<BlogCategory>)await blogCategoryRepo.FindAsync(b => b.IsDelete == false /*&& b.Enabled == true*/);
        }

        public async Task<TPaging<BlogCategoryResponse>> GetAvailableAsync(string searchTerm, Expression<Func<BlogCategory, bool>> expression,
            int pageIndex,
            int pageSize)
        {
            int index = 0;
            var cacheKeyCategory = BlogConstant.RedisKey.CACHE_KEY_BLOG_CATEGORY;
            var cachedCategoryEntities = await redisStogare.GetAsync<List<BlogCategory>>(cacheKeyCategory);
            var blogCategoryList = await GetBlogCategoryCacheAsync();

            if (cachedCategoryEntities != blogCategoryList)
            {
                cachedCategoryEntities = await GetBlogCategoryCacheAsync();
                await redisStogare.SetAsync(cacheKeyCategory, cachedCategoryEntities, TimeSpan.FromMinutes(30));
            }

            var totalRows = cachedCategoryEntities.Count();

            var category = cachedCategoryEntities
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                category = category.Where(s => s.Name.ToUpper().Contains(searchTerm.CustomTrim().ToUpper()) || s.Title.ToUpper().Contains(searchTerm.CustomTrim().ToUpper()) || s.Keyword.ToUpper().Contains(searchTerm.CustomTrim().ToUpper())).ToList();
            }

            return new TPaging<BlogCategoryResponse>
            {
                Source = category.Select(s => new BlogCategoryResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    CreatedDate = s.CreatedDate,
                    Slug = s.Slug,
                    Title = s.Title,
                    Thumbnail = s.Thumbnail,
                    Description = s.Description,
                    Keyword = s.Keyword,
                    Level = s.Level,
                    Enabled = s.Enabled,
                    Order = ++index,
                    ParentCategory = new NameValueModel<int?>
                    {
                        Name = s.ParentCategory?.Name,
                        Value = s.ParentCategory?.Id
                    }
                }),
                TotalRecords = totalRows
            };
        }

        public async Task<BaseResponse<object>> AddBlogCategoryAsync(BlogCategoryRequest model)
        {
            BlogCategory blogCategory = null;
            if (model.ParentId.HasValue)
            {
                blogCategory = await GetByIdAsync(model.ParentId.Value);
            }

            var entity = new BlogCategory
            {
                CreatedDate = DateTime.UtcNow,
                Name = model.Name,
                Slug = !string.IsNullOrWhiteSpace(model.Slug) ? model.Slug : model.Name.ToSlugUrl(),
                Thumbnail = model.Thumbnail,
                Enabled = model.Enabled,
                ParentId = model.ParentId,
                Order = model.Order,
                Level = StringExtension.MakeLevel(model.Order, blogCategory?.Level),
                Keyword = model.Keyword,
                Description = model.Description,
                Title = model.Title
            };

            try
            {
                await InsertAsync(entity);
                return new BaseResponse<object>
                {
                    Message = CommonMessage.INSERT_SUCCESS,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<BaseResponse<object>> UpdateBlogCategoryAsync(int id, BlogCategoryRequest model)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = CommonMessage.RECORD_NOT_FOUND
                };
            }

            BlogCategory blogCategory = null;
            if (model.ParentId.HasValue)
            {
                blogCategory = await GetByIdAsync(model.ParentId.Value);
            }

            entity.ModifiedDate = DateTime.UtcNow;
            entity.Name = model.Name;
            entity.Slug = !string.IsNullOrWhiteSpace(model.Slug) ? model.Slug : model.Name.ToSlugUrl();
            entity.Thumbnail = model.Thumbnail;
            entity.Enabled = model.Enabled;
            entity.ParentId = model.ParentId;
            entity.Order = model.Order;
            entity.Level = StringExtension.MakeLevel(model.Order, blogCategory?.Level);
            entity.Keyword = model.Keyword;
            entity.Description = model.Description;
            entity.Title = model.Title;

            try
            {
                await UpdateAsync(entity);
                return new BaseResponse<object>
                {
                    Message = CommonMessage.UPDATE_SUCCESS,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<TPaging<BlogCategoryApiResponse>> GetCategoriesAsync(string? name)
        {
            var blogCategory = new List<BlogCategoryApiResponse>();

            var cacheKeyCategory = BlogConstant.RedisKey.CACHE_KEY_BLOG_CATEGORY;
            var cachedCategoryEntities = await redisStogare.GetAsync<List<BlogCategory>>(cacheKeyCategory);

            if (cachedCategoryEntities == null)
            {
                cachedCategoryEntities = await GetBlogCategoryCacheAsync();

                await redisStogare.SetAsync(cacheKeyCategory, cachedCategoryEntities, TimeSpan.FromMinutes(30));
            }

            if (cachedCategoryEntities != null)
            {
                blogCategory = cachedCategoryEntities.Select(s => new BlogCategoryApiResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    Thumbnail = s.Thumbnail,
                }).ToList();
            }

            if (!string.IsNullOrEmpty(name))
            {
                blogCategory = blogCategory.Where(x => x.Name.Contains(name)).ToList();
            }

            return new TPaging<BlogCategoryApiResponse>
            {
                Source = blogCategory,
            };
        }

        public async Task<BaseResponse<object>> GetBlogCategoriesById(int blogId)
        {
            var categories = await blogCategoryMapRepo.FindAsync(s => s.BlogCategoryId == blogId && !s.IsDelete);

            if (categories == null)
            {
                return new BaseResponse<object>
                {
                    Success = false,
                    Message = BlogConstant.Message.CATEGORY_DO_NOT_EXIST,
                    MessageCode = nameof(BlogConstant.Message.CATEGORY_DO_NOT_EXIST)
                };
            }

            return new BaseResponse<object>
            {
                Success = true,
                Data = categories
            };
        }

        public async Task<BaseResponse<object>> DeleteCategoryAsync(int entityId)
        {
            var entity = await Repository.GetByIdAsync(entityId);
            entity.IsDelete = true;

            using (var scopre = new TransactionScope())
            {
                Repository.Update(entity);

                scopre.Complete();
            }

            return new BaseResponse<object>
            {
                Success = true,
                Message = CommonMessage.Account.DELETE_ACCOUNT_SUCCESSFUL,
                MessageCode = nameof(CommonMessage.Account.DELETE_ACCOUNT_SUCCESSFUL)
            };
        }
    }
}