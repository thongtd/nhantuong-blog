using DailyContentWriter.Models;
using MicroBase.RedisProvider;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyContentWriter.Services;

public class SchedulerWorker : BackgroundService
{
    private readonly ILogger<SchedulerWorker> logger;
    private readonly AppSettings settings;
    private readonly ContentJobService contentJobService;
    private readonly IRedisStogare redisStogare;

    private readonly HashSet<string> executedKeys = new();
    private const string LastRunRedisKey = "scheduler:contentjob:last_run";
    private static readonly TimeSpan JobInterval = TimeSpan.FromHours(1);

    public SchedulerWorker(
        ILogger<SchedulerWorker> logger,
        IOptions<AppSettings> options,
        ContentJobService contentJobService,
        IRedisStogare redisStogare)
    {
        this.logger = logger;
        settings = options.Value;
        this.contentJobService = contentJobService;
        this.redisStogare = redisStogare;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SchedulerWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Lấy mốc chạy cuối từ Redis
                var lastRunStr = await redisStogare.GetAsync<string>(LastRunRedisKey);

                DateTime? lastRun = null;
                if (!string.IsNullOrWhiteSpace(lastRunStr) &&
                    DateTime.TryParse(lastRunStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                {
                    lastRun = parsed;
                }

                var shouldRun = !lastRun.HasValue || (now - lastRun.Value) >= JobInterval;

                if (shouldRun)
                {
                    logger.LogInformation(
                        "Scheduler triggered at {Now}. LastRun = {LastRun}",
                        now,
                        lastRun);

                    await contentJobService.ProcessOnePendingRowAsync();

                    // Chỉ cập nhật sau khi chạy thành công
                    await redisStogare.SetAsync(
                        LastRunRedisKey,
                        now.ToString("O")); // ISO 8601 round-trip

                    logger.LogInformation("Scheduler completed. New LastRun = {Now}", now);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi chạy scheduler.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(settings.Schedule.CheckIntervalSeconds),
                stoppingToken);
        }
    }
}