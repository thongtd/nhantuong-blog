using MicroBase.FileManager;
using MicroBase.RedisProvider;
using MicroBase.Share;
using MicroBase.Share.Extensions;
using MicroBase.Share.Linqkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;
using MicroBase.Repository.Repositories;
using MicroBase.Share.Models.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using MicroBase.Service;
using Blog.Entity.Entities;
using Blog.Entity;
using Blog.Share.Models.CMS;
using Blog.Share.Models;
using Blog.Share;

namespace Blog.Service.API
{
    public interface ITagService : IGenericService<BlogTag, BlogDbContext>
    {
        Task<TPaging<TagCmsResponse>> GetTagsAsync(string searchTerm,
            int pageIndex,
            int pageSize);

        Task<List<BlogTagMapRequest>> GetTagsByBlogIdAsync(int blogId);

        Task<BaseResponse<object>> DeleteTagAsync(int entityId);

        Task<BaseResponse<bool>> SyncTagToCacheAsync();
    }

    public class TagService : GenericService<BlogTag, BlogDbContext>, ITagService
    {
        private readonly BlogDbContext microDbContext;
        private readonly ILogger<TagService> logger;
        private readonly IFileUploadService fileUploadService;
        private readonly IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo;
        private readonly IRepository<BlogTag, BlogDbContext> tagRepo;
        private readonly IRedisStogare redisStogare;

        public TagService(IRepository<BlogTag, BlogDbContext> repository,
            BlogDbContext microDbContext,
            ILogger<TagService> logger,
            IConfiguration configuration,
            IRedisStogare redisStogare,
            IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo,
            IRepository<BlogTag, BlogDbContext> tagRepo,
            FileUploadFactory fileUploadFactory)
            : base(repository)
        {
            this.microDbContext = microDbContext;
            this.logger = logger;
            this.blogTagMapRepo = blogTagMapRepo;
            this.tagRepo = tagRepo;
            this.redisStogare = redisStogare;
            var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
            fileUploadService = fileUploadFactory.GetServiceByName(uploadServiceName);
        }

        private async Task<List<BlogTag>> GetBlogTagCacheAsync()
        {
            return (List<BlogTag>)await tagRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);
        }

        public async Task<BaseResponse<bool>> SyncTagToCacheAsync()
        {
            try
            {
                var cacheKeyBlogTag = BlogConstant.RedisKey.CACHE_KEY_BLOG_TAG;
                var cachedBlogTagEntities = await GetBlogTagCacheAsync();

                await redisStogare.SetAsync(cacheKeyBlogTag, cachedBlogTagEntities);

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = BlogConstant.CommonMessageBlog.SYNC_BLOG_TAG_SUCCESS,
                    MessageCode = nameof(BlogConstant.CommonMessageBlog.SYNC_BLOG_TAG_SUCCESS)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new BaseResponse<bool>
                {
                    Success = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = CommonMessage.UN_DETECTED_ERROR,
                    MessageCode = nameof(CommonMessage.UN_DETECTED_ERROR)
                };
            }
        }

        public async Task<TPaging<TagCmsResponse>> GetTagsAsync(string searchTerm, int pageIndex, int pageSize)
        {
            try
            {
                int index = 0;
                var cacheKeyTags = "tagEntitiesKey";
                var cachedTagEntities = await redisStogare.GetAsync<List<BlogTag>>(cacheKeyTags);
                var listTag = await microDbContext.Set<BlogTag>()
                    .Where(s => s.IsDelete == false)
                    .OrderByDescending(s => s.CreatedDate)
                    .ToListAsync();

                if (cachedTagEntities != listTag)
                {
                    cachedTagEntities = await microDbContext.Set<BlogTag>()
                        .Where(s => s.IsDelete == false)
                        .OrderByDescending(s => s.CreatedDate)
                        .ToListAsync();

                    await redisStogare.SetAsync(cacheKeyTags, cachedTagEntities, TimeSpan.FromMinutes(30));
                }

                var predicate = PredicateBuilder
                    .Create<BlogTag>(s => !s.IsDelete);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.CustomTrim().ToUpper();
                    cachedTagEntities = cachedTagEntities
                        .Where(s => s.Keyword.ToUpper().Contains(searchTerm) || s.Description.ToUpper().Contains(searchTerm)).ToList();
                }

                var tagEntities = cachedTagEntities
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                int totalRows = cachedTagEntities.Count();

                var res = new List<TagCmsResponse>();
                foreach (var tag in tagEntities)
                {
                    res.Add(new TagCmsResponse
                    {
                        Order = ++index,
                        Id = tag.Id,
                        Enabled = tag.Enabled,
                        Description = tag.Description,
                        CreatedDate = tag.CreatedDate,
                        ModifiedDate = tag.ModifiedDate,
                        Keyword = tag.Keyword,
                    });
                }

                return new TPaging<TagCmsResponse>
                {
                    Source = res,
                    TotalRecords = totalRows
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new TPaging<TagCmsResponse>
                {
                    Source = new List<TagCmsResponse>(),
                    TotalRecords = 0
                };
            }
        }

        public async Task<List<BlogTagMapRequest>> GetTagsByBlogIdAsync(int blogId)
        {
            var blogs = await blogTagMapRepo.FindAsync(s => s.BlogId == blogId && !s.IsDelete && s.Enabled == true);
            var tags = await tagRepo.FindAsync(s => s.IsDelete == false && s.Enabled == true);

            var blogCategoryMap = new List<BlogTagMapRequest>();
            if (blogs == null)
            {
                return blogCategoryMap;
            }

            foreach (var item in blogs)
            {
                var tagInfo = tags.FirstOrDefault(s => s.Id == item.TagId);
                blogCategoryMap.Add(new BlogTagMapRequest
                {
                    TagId = item.TagId,
                    Name = tagInfo != null ? tagInfo.Keyword : string.Empty
                });
            }

            return blogCategoryMap;
        }

        public async Task<BaseResponse<object>> DeleteTagAsync(int entityId)
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