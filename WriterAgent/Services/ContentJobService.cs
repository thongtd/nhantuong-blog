using Blog.Entity;
using Blog.Entity.Entities;
using Blog.Service.API;
using MicroBase.FileManager;
using MicroBase.Repository.Repositories;
using MicroBase.Share.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DailyContentWriter.Services;

public class ContentJobService
{
    private readonly GoogleSheetService googleSheetService;
    private readonly OpenAiService openAiService;
    private readonly ContentTransformer contentTransformer;
    private readonly ILogger<ContentJobService> logger;
    private readonly IRepository<Blog.Entity.Entities.Blog, BlogDbContext> blogRepo;
    private readonly IRepository<BlogTag, BlogDbContext> blogTagRepo;
    private readonly IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo;
    private readonly IFileUploadService fileUploadService;
    private readonly IRepository<BlogCategory, BlogDbContext> blogCategoryRepo;
    private readonly IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo;
    private readonly IBlogCacheService blogCacheService;
    private readonly LlmUsageCsvLogger llmUsageCsvLogger;
    private readonly GoogleIndexingService googleIndexingService;

    public ContentJobService(
        GoogleSheetService googleSheetService,
        OpenAiService openAiService,
        ContentTransformer contentTransformer,
        ILogger<ContentJobService> logger,
        IRepository<Blog.Entity.Entities.Blog, BlogDbContext> blogRepo,
        IRepository<BlogTag, BlogDbContext> blogTagRepo,
        IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo,
        IConfiguration configuration,
        FileUploadFactory fileUploadFactory,
        IRepository<BlogCategory, BlogDbContext> blogCategoryRepo,
        IRepository<BlogCategoryMap, BlogDbContext> blogCategoryMapRepo,
        IBlogCacheService blogCacheService,
        LlmUsageCsvLogger llmUsageCsvLogger,
        GoogleIndexingService googleIndexingService)
    {
        this.googleSheetService = googleSheetService;
        this.openAiService = openAiService;
        this.contentTransformer = contentTransformer;
        this.logger = logger;
        this.blogRepo = blogRepo;
        this.blogTagRepo = blogTagRepo;
        this.blogTagMapRepo = blogTagMapRepo;
        this.blogCategoryRepo = blogCategoryRepo;
        this.blogCacheService = blogCacheService;

        var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
        this.fileUploadService = fileUploadFactory.GetServiceByName(uploadServiceName);
        this.blogCategoryMapRepo = blogCategoryMapRepo;
        this.llmUsageCsvLogger = llmUsageCsvLogger;
        this.googleIndexingService = googleIndexingService;
    }

    private string GetFileExtensionFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var extension = Path.GetExtension(uri.AbsolutePath);

            if (!string.IsNullOrWhiteSpace(extension))
            {
                return extension;
            }

            return "jpg";
        }
        catch
        {
        }

        return null;
    }

    async Task<string> UploadImageAsync(string urlImage, string fileName)
    {
        var res = await fileUploadService.SaveFileFormUrlAsync(urlImage, fileName, string.Empty);
        if (res == null || res.Data == null)
        {
            return string.Empty;
        }

        return res.Data.FileUrl;
    }

    public async Task ProcessOnePendingRowAsync()
    {
        try
        {
            var rows = await googleSheetService.GetAllRowsAsync();

            var targetRow = rows.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r.MainContent) &&
                NormalizeStatus(r.Status) == "chưa viết");

            if (targetRow == null)
            {
                logger.LogInformation("Không còn dòng nào có trạng thái 'chưa viết'.");
                return;
            }

            logger.LogInformation("Đang xử lý dòng {RowNumber} - STT: {STT}", targetRow.RowNumber, targetRow.STT);

            var categoryEntity = await blogCategoryRepo.FindOneAsync(s => s.Name == targetRow.BlogCategory);
            if (categoryEntity == null)
            {
                return;
            }

            var noOfImg = 0;
            if (!string.IsNullOrWhiteSpace(targetRow.Image1))
            {
                noOfImg++;
            }

            if (!string.IsNullOrWhiteSpace(targetRow.Image2))
            {
                noOfImg++;
            }

            if (!string.IsNullOrWhiteSpace(targetRow.Image3))
            {
                noOfImg++;
            }

            var generationResult = await openAiService.GenerateArticleAsync(targetRow.MainContent, targetRow.SEOKeywords, noOfImg);
            var article = generationResult.Article;
            var htmlBody = contentTransformer.ConvertMarkdownToHtml(article.MarkdownBody ?? string.Empty);

            llmUsageCsvLogger.Log(
                generationResult.Model,
                article.Title,
                generationResult.PromptTokens,
                generationResult.CompletionTokens,
                generationResult.TotalTokens);

            var finalHtml = htmlBody;

            string urlImage1 = string.Empty,
                urlImage2 = string.Empty,
                urlImage3 = string.Empty;

            var title = article.Title;
            var slug = title.Trim().ToSlugUrl();

            if (!string.IsNullOrWhiteSpace(targetRow.Image1))
            {
                urlImage1 = await UploadImageAsync(targetRow.Image1, $"{slug}.{GetFileExtensionFromUrl(targetRow.Image1)}");
            }

            if (!string.IsNullOrWhiteSpace(targetRow.Image2))
            {
                urlImage2 = await UploadImageAsync(targetRow.Image2, $"{slug}-1.{GetFileExtensionFromUrl(targetRow.Image2)}");
            }

            if (!string.IsNullOrWhiteSpace(targetRow.Image3))
            {
                urlImage3 = await UploadImageAsync(targetRow.Image3, $"{slug}-2.{GetFileExtensionFromUrl(targetRow.Image3)}");
            }

            finalHtml = finalHtml.Replace("ANH_1", $"<img src=\"{urlImage1}\" alt=\"{title}\">");
            finalHtml = finalHtml.Replace("ANH_2", $"<img src=\"{urlImage2}\" alt=\"{title}\">");
            finalHtml = finalHtml.Replace("ANH_3", $"<img src=\"{urlImage3}\" alt=\"{title}\">");

            var blogEntity = new Blog.Entity.Entities.Blog
            {
                Name = title,
                BodyContent = finalHtml,
                SubContent = article.SubContent,
                Slug = slug,
                CreatedDate = DateTime.UtcNow.UtcToVietnamTime(),
                Enabled = true,
                HotNews = true,
                IsDelete = false,
                Title = title,
                Thumbnail = targetRow.Thumbnail,
            };

            await blogRepo.InsertAsync(blogEntity);

            await blogCategoryMapRepo.InsertAsync(new BlogCategoryMap
            {
                BlogCategoryId = categoryEntity.Id,
                BlogId = blogEntity.Id,
                CreatedDate = DateTime.UtcNow.UtcToVietnamTime(),
                IsDelete = false,
                Enabled = true,
            });

            var tagMapEntities = new List<BlogTagMap>();

            var tags = targetRow.BlogTags.Split(',');
            foreach (var tag in tags)
            {
                var tagSlug = tag.Trim().ToSlugUrl();
                var tagEntity = await blogTagRepo.FindOneAsync(s => s.Slug == tagSlug);
                if (tagEntity == null)
                {
                    tagEntity = new BlogTag
                    {
                        Keyword = tag,
                        Slug = tagSlug,
                        NomalizationKeyword = tag.ToLowerInvariant(),
                        IsDelete = false,
                        CreatedDate = DateTime.UtcNow.UtcToVietnamTime(),
                        Order = 1,
                        Enabled = true
                    };

                    await blogTagRepo.InsertAsync(tagEntity);
                }

                tagMapEntities.Add(new BlogTagMap
                {
                    BlogId = blogEntity.Id,
                    TagId = tagEntity.Id,
                    IsDelete = false,
                    CreatedDate = DateTime.UtcNow.UtcToVietnamTime(),
                    Enabled = true
                });
            }

            await blogTagMapRepo.InsertManyAsync(tagMapEntities);

            await blogCacheService.BuildBlogsToCacheAsync(blogEntity.Id);
            await blogCacheService.BuildTagsToCacheAsync();

            logger.LogInformation("Đã tạo xong HTML để lưu DB. Title: {Title}", title);

            await googleSheetService.UpdateStatusAsync(targetRow.RowNumber, "Đã viết");

            logger.LogInformation("Đã cập nhật trạng thái dòng {RowNumber} thành 'Đã viết'.", targetRow.RowNumber);

            // Thông báo Google Indexing API về URL mới
            var blogUrl = $"https://nhantuong.vn/{slug}";
            await googleIndexingService.NotifyUrlUpdatedAsync(blogUrl);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private static string NormalizeStatus(string status)
    {
        status = (status ?? string.Empty).Trim().ToLowerInvariant();
        status = status.Replace("đ", "d");
        return status;
    }
}