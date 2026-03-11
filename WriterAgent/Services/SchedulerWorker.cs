using DailyContentWriter.Models;
using DailyContentWriter.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyContentWriter.Services;

public class SchedulerWorker : BackgroundService
{
    private readonly ILogger<SchedulerWorker> _logger;
    private readonly AppSettings _settings;
    private readonly ContentJobService _contentJobService;

    private readonly HashSet<string> _executedKeys = new();

    public SchedulerWorker(
        ILogger<SchedulerWorker> logger,
        IOptions<AppSettings> options,
        ContentJobService contentJobService)
    {
        _logger = logger;
        _settings = options.Value;
        _contentJobService = contentJobService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SchedulerWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var executionKey = TimeScheduleHelper.BuildExecutionKey(now);

                //if (TimeScheduleHelper.IsScheduledTime(now, _settings.Schedule.Times))
                //{
                //    if (!_executedKeys.Contains(executionKey))
                //    {
                _logger.LogInformation("Triggered at {Now}", now);
                        await _contentJobService.ProcessOnePendingRowAsync();
                        _executedKeys.Add(executionKey);
                //    }
                //}

                CleanupOldKeys();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chạy scheduler.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.Schedule.CheckIntervalSeconds), stoppingToken);
        }
    }

    private void CleanupOldKeys()
    {
        if (_executedKeys.Count < 500) return;

        var todayPrefix = DateTime.Now.ToString("yyyy-MM-dd");
        var oldKeys = _executedKeys.Where(x => !x.StartsWith(todayPrefix)).ToList();

        foreach (var key in oldKeys)
        {
            _executedKeys.Remove(key);
        }
    }
}