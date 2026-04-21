using DailyContentWriter.Models;

namespace WriterAgent.Models;

public class GenerationResult
{
    public ArticleResult Article { get; set; }

    public string Model { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }
}
