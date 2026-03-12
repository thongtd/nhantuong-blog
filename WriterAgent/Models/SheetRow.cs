namespace DailyContentWriter.Models;

public class SheetRow
{
    public int RowNumber { get; set; }
    
    public string STT { get; set; } = string.Empty;
    
    public string Thumbnail { get; set; } = string.Empty;

    public string MainContent { get; set; } = string.Empty;

    public string SEOKeywords { get; set; } = string.Empty;
    
    public string BlogTags { get; set; } = string.Empty;

    public string Image1 { get; set; } = string.Empty;
    
    public string Image2 { get; set; } = string.Empty;
    
    public string Image3 { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;

    public string BlogCategory { get; set; } = string.Empty;
}