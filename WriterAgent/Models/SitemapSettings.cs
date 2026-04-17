namespace DailyContentWriter.Models;

public class SitemapSettings
{
    public string Domain { get; set; } = "https://nhantuong.vn";

    public string OutputDir { get; set; } = string.Empty;

    public string DestinationDir { get; set; } = string.Empty;

    public string ScheduleTime { get; set; } = "00:00";
}
