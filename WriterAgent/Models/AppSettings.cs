namespace DailyContentWriter.Models;

public class AppSettings
{
    public GoogleSheetSettings GoogleSheet { get; set; } = new();

    public OpenAiSettings ChatGpt { get; set; } = new();

    public ScheduleSettings Schedule { get; set; } = new();
}

public class GoogleSheetSettings
{
    public string SpreadsheetId { get; set; } = string.Empty;

    public string SheetName { get; set; } = "Sheet1";

    public string ServiceAccountFile { get; set; } = "service-account.json";
}

public class OpenAiSettings
{
    public string Token { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-5.4";

    public string ApiUrl { get; set; }
}

public class ScheduleSettings
{
    public List<string> Times { get; set; } = new();

    public int CheckIntervalSeconds { get; set; } = 30;
}