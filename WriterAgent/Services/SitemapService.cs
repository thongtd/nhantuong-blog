using Blog.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DailyContentWriter.Models;
using System.Text;

namespace DailyContentWriter.Services;

public class SitemapService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SitemapService> _logger;
    private readonly AppSettings _settings;

    private const string SitemapIndexFile = "sitemap.xml";
    private const string BlogSitemapFile = "blog_sitemap.xml";
    private const string TagsSitemapFile = "tags_sitemap.xml";
    private const string CategorySitemapFile = "category_sitemap.xml";

    private const string UrlsetOpen = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n";
    private const string UrlsetClose = "</urlset>";

    public SitemapService(
        IServiceScopeFactory scopeFactory,
        ILogger<SitemapService> logger,
        IOptions<AppSettings> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task ExecuteAsync()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var sitemap = _settings.Sitemap;
        var outputDir = sitemap.OutputDir;
        var destDir = sitemap.DestinationDir;

        _logger.LogInformation("══ SITEMAP JOB START ══");
        _logger.LogInformation("  Config: Domain={Domain} | OutputDir={OutputDir} | DestDir={DestDir}",
            sitemap.Domain, outputDir, destDir);

        Directory.CreateDirectory(outputDir);

        var blogCount = await AppendBlogEntries(outputDir, sitemap.Domain);
        var tagCount = await AppendTagEntries(outputDir, sitemap.Domain);
        var catCount = await AppendCategoryEntries(outputDir, sitemap.Domain);

        EnsureSitemapIndex(outputDir, sitemap.Domain);

        CopyToDestination(outputDir, destDir);

        sw.Stop();
        _logger.LogInformation("══ SITEMAP JOB END ══ Blogs={Blogs} | Tags={Tags} | Categories={Categories} | Duration={Duration}ms",
            blogCount, tagCount, catCount, sw.ElapsedMilliseconds);
    }

    private async Task<int> AppendBlogEntries(string outputDir, string domain)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        var newBlogs = await db.Blogs
            .Where(b => b.Enabled && !b.IsDelete && !b.SitemapIndexed)
            .Select(b => new { b.Id, b.Slug, b.ModifiedDate, b.CreatedDate })
            .ToListAsync();

        _logger.LogInformation("  [BLOGS] Query: found {Count} new records", newBlogs.Count);

        if (newBlogs.Count == 0)
            return 0;

        var filePath = Path.Combine(outputDir, BlogSitemapFile);
        var entries = new StringBuilder();
        var skipped = 0;

        foreach (var blog in newBlogs)
        {
            if (string.IsNullOrWhiteSpace(blog.Slug))
            {
                skipped++;
                _logger.LogWarning("  [BLOGS] Skipped blog id={Id} (empty slug)", blog.Id);
                continue;
            }

            var lastmod = (blog.ModifiedDate ?? blog.CreatedDate).ToString("yyyy-MM-dd");
            entries.AppendLine($"  <url><loc>{domain}/{blog.Slug}</loc><lastmod>{lastmod}</lastmod><changefreq>weekly</changefreq><priority>0.7</priority></url>");
        }

        AppendToSitemapFile(filePath, entries.ToString());

        var ids = newBlogs.Select(b => b.Id).ToList();
        await MarkIndexed(db, "blogs", ids);

        _logger.LogInformation("  [BLOGS] Appended {Count} URLs to {File} (skipped={Skipped})",
            newBlogs.Count - skipped, BlogSitemapFile, skipped);

        return newBlogs.Count;
    }

    private async Task<int> AppendTagEntries(string outputDir, string domain)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        var newTags = await db.BlogTags
            .Where(t => t.Enabled && !t.IsDelete && !t.SitemapIndexed)
            .Select(t => new { t.Id, t.Slug, t.ModifiedDate, t.CreatedDate })
            .ToListAsync();

        _logger.LogInformation("  [TAGS] Query: found {Count} new records", newTags.Count);

        if (newTags.Count == 0)
            return 0;

        var filePath = Path.Combine(outputDir, TagsSitemapFile);
        var entries = new StringBuilder();
        var skipped = 0;

        foreach (var tag in newTags)
        {
            if (string.IsNullOrWhiteSpace(tag.Slug))
            {
                skipped++;
                _logger.LogWarning("  [TAGS] Skipped tag id={Id} (empty slug)", tag.Id);
                continue;
            }

            var lastmod = (tag.ModifiedDate ?? tag.CreatedDate).ToString("yyyy-MM-dd");
            entries.AppendLine($"  <url><loc>{domain}/tag/{tag.Slug}</loc><lastmod>{lastmod}</lastmod><changefreq>daily</changefreq><priority>0.6</priority></url>");
        }

        AppendToSitemapFile(filePath, entries.ToString());

        var ids = newTags.Select(t => t.Id).ToList();
        await MarkIndexed(db, "blog_tags", ids);

        _logger.LogInformation("  [TAGS] Appended {Count} URLs to {File} (skipped={Skipped})",
            newTags.Count - skipped, TagsSitemapFile, skipped);

        return newTags.Count;
    }

    private async Task<int> AppendCategoryEntries(string outputDir, string domain)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        var newCats = await db.BlogCategories
            .Where(c => c.Enabled && !c.IsDelete && !c.SitemapIndexed)
            .Select(c => new { c.Id, c.Slug, c.ModifiedDate, c.CreatedDate })
            .ToListAsync();

        _logger.LogInformation("  [CATEGORIES] Query: found {Count} new records", newCats.Count);

        if (newCats.Count == 0)
            return 0;

        var filePath = Path.Combine(outputDir, CategorySitemapFile);
        var entries = new StringBuilder();
        var skipped = 0;

        foreach (var cat in newCats)
        {
            if (string.IsNullOrWhiteSpace(cat.Slug))
            {
                skipped++;
                _logger.LogWarning("  [CATEGORIES] Skipped category id={Id} (empty slug)", cat.Id);
                continue;
            }

            var lastmod = (cat.ModifiedDate ?? cat.CreatedDate).ToString("yyyy-MM-dd");
            entries.AppendLine($"  <url><loc>{domain}/{cat.Slug}</loc><lastmod>{lastmod}</lastmod><changefreq>daily</changefreq><priority>0.8</priority></url>");
        }

        AppendToSitemapFile(filePath, entries.ToString());

        var ids = newCats.Select(c => c.Id).ToList();
        await MarkIndexed(db, "blog_categories", ids);

        _logger.LogInformation("  [CATEGORIES] Appended {Count} URLs to {File} (skipped={Skipped})",
            newCats.Count - skipped, CategorySitemapFile, skipped);

        return newCats.Count;
    }

    private static async Task MarkIndexed(BlogDbContext db, string tableName, List<int> ids)
    {
        if (ids.Count == 0)
            return;

        var idList = string.Join(",", ids);
        await db.Database.ExecuteSqlRawAsync(
            $"UPDATE {tableName} SET sitemap_indexed = 1 WHERE id IN ({idList})");
    }

    private void AppendToSitemapFile(string filePath, string newEntries)
    {
        if (string.IsNullOrWhiteSpace(newEntries))
            return;

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, UrlsetOpen + newEntries + UrlsetClose, Encoding.UTF8);
            return;
        }

        var content = File.ReadAllText(filePath, Encoding.UTF8);
        var closeTagIndex = content.LastIndexOf(UrlsetClose, StringComparison.Ordinal);

        if (closeTagIndex < 0)
        {
            _logger.LogWarning("Invalid sitemap file {File}, regenerating", filePath);
            File.WriteAllText(filePath, UrlsetOpen + newEntries + UrlsetClose, Encoding.UTF8);
            return;
        }

        var updated = string.Concat(content.AsSpan(0, closeTagIndex), newEntries, UrlsetClose);
        File.WriteAllText(filePath, updated, Encoding.UTF8);
    }

    private void EnsureSitemapIndex(string outputDir, string domain)
    {
        var filePath = Path.Combine(outputDir, SitemapIndexFile);
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        sb.AppendLine();

        // Static pages
        sb.AppendLine("  <!-- Trang chủ & trang tĩnh -->");
        AppendStaticUrls(sb, domain);
        sb.AppendLine();

        // Sub-sitemaps
        sb.AppendLine("  <!-- Sub-sitemaps -->");
        AppendSitemapRef(sb, domain, CategorySitemapFile, now);
        AppendSitemapRef(sb, domain, TagsSitemapFile, now);
        AppendSitemapRef(sb, domain, BlogSitemapFile, now);
        sb.AppendLine();

        sb.AppendLine("</sitemapindex>");

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static void AppendSitemapRef(StringBuilder sb, string domain, string fileName, string lastmod)
    {
        sb.AppendLine($"  <sitemap>");
        sb.AppendLine($"    <loc>{domain}/{fileName}</loc>");
        sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
        sb.AppendLine($"  </sitemap>");
    }

    private static void AppendStaticUrls(StringBuilder sb, string domain)
    {
        var staticPages = new (string path, string priority, string freq)[]
        {
            ("/", "1.0", "weekly"),
            ("/luan-giai-nhan-tuong-mau", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-mien-phi", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-kham-pha-ban-menh", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-kham-pha-ban-menh", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-hoi-viec-cong-danh-su-nghiep", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-hoi-viec-cong-danh-su-nghiep", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-tai-loc-quan-lo", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-tai-loc-quan-lo", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-tinh-duyen-hon-nhan-gia-dinh", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-tinh-duyen-hon-nhan-gia-dinh", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-gia-dao-tinh-duyen-phuc-duc-hau-van", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-gia-dao-tinh-duyen-phuc-duc-hau-van", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-menh-tai-quan", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-menh-tai-quan", "0.9", "weekly"),
            ("/goi-luan-giai-tu-vi/xem-tu-vi-chuyen-sau-12-cung-la-so", "0.9", "weekly"),
            ("/tu-vi-mau/xem-tu-vi-chuyen-sau-12-cung-la-so", "0.9", "weekly"),
        };

        foreach (var (path, priority, freq) in staticPages)
        {
            sb.AppendLine($"  <!-- static --><url><loc>{domain}{path}</loc><priority>{priority}</priority><changefreq>{freq}</changefreq></url>");
        }
    }

    private void CopyToDestination(string outputDir, string destDir)
    {
        if (string.IsNullOrWhiteSpace(destDir))
        {
            _logger.LogWarning("DestinationDir not configured, skipping copy");
            return;
        }

        Directory.CreateDirectory(destDir);

        var files = new[] { SitemapIndexFile, BlogSitemapFile, TagsSitemapFile, CategorySitemapFile };
        foreach (var file in files)
        {
            var src = Path.Combine(outputDir, file);
            if (!File.Exists(src))
                continue;

            var dest = Path.Combine(destDir, file);
            File.Copy(src, dest, overwrite: true);
            _logger.LogInformation("Copied {File} → {Dest}", file, dest);
        }
    }
}
