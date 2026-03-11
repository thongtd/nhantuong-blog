using Blog.Entity;
using Blog.Entity.Entities;
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

    public ContentJobService(
        GoogleSheetService googleSheetService,
        OpenAiService openAiService,
        ContentTransformer contentTransformer,
        ILogger<ContentJobService> logger,
        IRepository<Blog.Entity.Entities.Blog, BlogDbContext> blogRepo,
        IRepository<BlogTag, BlogDbContext> blogTagRepo,
        IRepository<BlogTagMap, BlogDbContext> blogTagMapRepo,
        IConfiguration configuration,
        FileUploadFactory fileUploadFactory)
    {
        this.googleSheetService = googleSheetService;
        this.openAiService = openAiService;
        this.contentTransformer = contentTransformer;
        this.logger = logger;
        this.blogRepo = blogRepo;
        this.blogTagRepo = blogTagRepo;
        this.blogTagMapRepo = blogTagMapRepo;

        var uploadServiceName = configuration.GetValue<string>("FileManage:EnableService");
        this.fileUploadService = fileUploadFactory.GetServiceByName(uploadServiceName);
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
        return res.Data.FileUrl;
    }

    public async Task ProcessOnePendingRowAsync()
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

        var article = await openAiService.GenerateArticleAsync(targetRow.MainContent, targetRow.SEOKeywords, noOfImg);
        var htmlBody = contentTransformer.ConvertMarkdownToHtml(article.MarkdownBody ?? string.Empty);

        var finalHtml = contentTransformer.InsertImagesToHtml(
            htmlBody,
            targetRow.Image1,
            targetRow.Image2,
            targetRow.Image3);

        string urlImage1 = string.Empty,
            urlImage2 = string.Empty,
            urlImage3 = string.Empty;

        var title = article.Title;
        var slug = title.ToSlugUrl();

        if (!string.IsNullOrWhiteSpace(targetRow.Image1))
        {
            urlImage1 = await UploadImageAsync(targetRow.Image1, $"{slug}.{GetFileExtensionFromUrl(targetRow.Image1)}");
        }

        if (!string.IsNullOrWhiteSpace(targetRow.Image2))
        {
            urlImage2 = await UploadImageAsync(targetRow.Image2, $"{slug}.{GetFileExtensionFromUrl(targetRow.Image2)}");
        }

        if (!string.IsNullOrWhiteSpace(targetRow.Image3))
        {
            urlImage3 = await UploadImageAsync(targetRow.Image3, $"{slug}.{GetFileExtensionFromUrl(targetRow.Image3)}");
        }

        finalHtml = finalHtml.Replace("ANH_1", urlImage1);
        finalHtml = finalHtml.Replace("ANH_2", urlImage2);
        finalHtml = finalHtml.Replace("ANH_3", urlImage3);

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

        var tagMapEntities = new List<BlogTagMap>();

        var tags = targetRow.BlogTags.Split(',');
        foreach (var tag in tags)
        {
            var tagSlug = tag.ToSlugUrl();
            var tagEntity = await blogTagRepo.FindOneAsync(s => s.Slug == tagSlug);
            if (tagEntity == null)
            {
                tagEntity = new BlogTag
                {
                    Keyword = tag,
                    Slug = tagSlug,
                    NomalizationKeyword = tag.ToLowerInvariant(),
                    IsDelete = false,
                    CreatedDate = DateTime.UtcNow.UtcToVietnamTime()
                };

                await blogTagRepo.InsertAsync(tagEntity);
            }

            tagMapEntities.Add(new BlogTagMap
            {
                BlogId = blogEntity.Id,
                TagId = tagEntity.Id,
                IsDelete = false,
                CreatedDate = DateTime.UtcNow.UtcToVietnamTime()
            });
        }

        await blogTagMapRepo.InsertManyAsync(tagMapEntities);

        logger.LogInformation("Đã tạo xong HTML để lưu DB. Title: {Title}", title);

        await googleSheetService.UpdateStatusAsync(targetRow.RowNumber, "Đã viết");

        logger.LogInformation("Đã cập nhật trạng thái dòng {RowNumber} thành 'Đã viết'.", targetRow.RowNumber);
    }

    private static string NormalizeStatus(string status)
    {
        status = (status ?? string.Empty).Trim().ToLowerInvariant();
        status = status.Replace("đ", "d");
        return status;
    }
}