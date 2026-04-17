using DailyContentWriter.Models;
using MicroBase.RedisProvider;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyContentWriter.Services;

public class SitemapSchedulerWorker : BackgroundService
{
    private readonly ILogger<SitemapSchedulerWorker> _logger;
    private readonly SitemapService _sitemapService;
    private readonly IRedisStogare _redis;
    private readonly AppSettings _settings;

    private const string LastRunRedisKey = "sitemap:last_run_date";

    public SitemapSchedulerWorker(
        ILogger<SitemapSchedulerWorker> logger,
        SitemapService sitemapService,
        IRedisStogare redis,
        IOptions<AppSettings> options)
    {
        _logger = logger;
        _sitemapService = sitemapService;
        _redis = redis;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SitemapSchedulerWorker started. Schedule: {Time}",
            _settings.Sitemap.ScheduleTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var todayKey = now.ToString("yyyy-MM-dd");

                if (!TimeSpan.TryParse(_settings.Sitemap.ScheduleTime, out var scheduleTime))
                    scheduleTime = TimeSpan.Zero;

                var isPastSchedule = now.TimeOfDay >= scheduleTime;

                if (isPastSchedule)
                {
                    var lastRun = await _redis.GetAsync<string>(LastRunRedisKey);

                    if (lastRun != todayKey)
                    {
                        _logger.LogInformation("Sitemap job triggered at {Now}", now);

                        await _sitemapService.ExecuteAsync();

                        await _redis.SetAsync(LastRunRedisKey, todayKey);

                        _logger.LogInformation("Sitemap job completed for {Date}", todayKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sitemap scheduler error");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
