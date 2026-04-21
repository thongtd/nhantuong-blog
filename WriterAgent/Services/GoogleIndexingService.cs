using DailyContentWriter.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DailyContentWriter.Services;

public class GoogleIndexingService
{
    private const string IndexingApiUrl = "https://indexing.googleapis.com/v3/urlNotifications:publish";
    private const string SearchConsoleApiUrl = "https://www.googleapis.com/webmasters/v3/sites";
    private const string IndexingScope = "https://www.googleapis.com/auth/indexing";
    private const string WebmastersScope = "https://www.googleapis.com/auth/webmasters";

    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleIndexingService> _logger;

    public GoogleIndexingService(
        IOptions<AppSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleIndexingService> logger)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Thong bao Google ve URL moi/cap nhat qua Indexing API.
    /// Google se uu tien crawl URL nay som hon.
    /// </summary>
    public async Task NotifyUrlUpdatedAsync(string url)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(IndexingScope);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = JsonSerializer.Serialize(new
            {
                url,
                type = "URL_UPDATED"
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(IndexingApiUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Indexing API: Notified URL_UPDATED for {Url}", url);
            }
            else
            {
                _logger.LogWarning("Indexing API: Failed for {Url} - {Status} - {Body}",
                    url, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Indexing API: Error notifying {Url}", url);
        }
    }

    /// <summary>
    /// Gui thong bao cho nhieu URL cung luc (batch).
    /// Google Indexing API cho phep toi da 200 requests/ngay.
    /// </summary>
    public async Task NotifyUrlsUpdatedAsync(IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            await NotifyUrlUpdatedAsync(url);
            await Task.Delay(500);
        }
    }

    /// <summary>
    /// Submit sitemap qua Google Search Console API.
    /// </summary>
    public async Task SubmitSitemapAsync(string sitemapUrl)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(WebmastersScope);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var siteUrl = Uri.EscapeDataString(_settings.Sitemap.Domain);
            var feedPath = Uri.EscapeDataString(sitemapUrl);
            var apiUrl = $"{SearchConsoleApiUrl}/{siteUrl}/sitemaps/{feedPath}";

            var response = await client.PutAsync(apiUrl, null);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Search Console: Submitted sitemap {Sitemap}", sitemapUrl);
            }
            else
            {
                _logger.LogWarning("Search Console: Failed to submit {Sitemap} - {Status} - {Body}",
                    sitemapUrl, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search Console: Error submitting sitemap {Sitemap}", sitemapUrl);
        }
    }

    /// <summary>
    /// Submit tat ca sitemaps (index + sub-sitemaps).
    /// </summary>
    public async Task SubmitAllSitemapsAsync()
    {
        var domain = _settings.Sitemap.Domain;
        var sitemaps = new[]
        {
            $"{domain}/sitemap.xml",
            $"{domain}/blog_sitemap.xml",
            $"{domain}/tags_sitemap.xml",
            $"{domain}/category_sitemap.xml",
            $"{domain}/static_sitemap.xml"
        };

        foreach (var sitemap in sitemaps)
        {
            await SubmitSitemapAsync(sitemap);
            await Task.Delay(300);
        }
    }

    private async Task<string> GetAccessTokenAsync(string scope)
    {
        var serviceAccountPath = Path.Combine(
            AppContext.BaseDirectory, _settings.GoogleSheet.ServiceAccountFile);

        using var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read);
        var credential = GoogleCredential.FromStream(stream).CreateScoped(scope);

        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        return accessToken;
    }
}
