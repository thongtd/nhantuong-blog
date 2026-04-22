using System.Globalization;
using DailyContentWriter.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyContentWriter.Services;

public class LlmUsageCsvLogger
{
    private const string CsvFileName = "llm_usage_log.csv";
    private const string CsvHeader = "DateTime,Model,Title,PromptTokens,CompletionTokens,TotalTokens";

    private readonly string _csvFilePath;
    private readonly ILogger<LlmUsageCsvLogger> _logger;
    private readonly object _lock = new();

    public LlmUsageCsvLogger(ILogger<LlmUsageCsvLogger> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        var outputDir = options.Value.Sitemap.OutputDir;
        _csvFilePath = Path.Combine(AppContext.BaseDirectory, outputDir, CsvFileName);
    }

    public void Log(string model, string title, int promptTokens, int completionTokens, int totalTokens)
    {
        lock (_lock)
        {
            var fileExists = File.Exists(_csvFilePath);

            using var writer = new StreamWriter(_csvFilePath, append: true);

            if (!fileExists)
            {
                writer.WriteLine(CsvHeader);
            }

            var now = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var escapedTitle = EscapeCsvField(title);

            writer.WriteLine($"{now},{model},{escapedTitle},{promptTokens},{completionTokens},{totalTokens}");

            _logger.LogInformation(
                "LLM Usage logged - Model: {Model}, Title: {Title}, Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
                model, title, promptTokens, completionTokens, totalTokens);
        }
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
