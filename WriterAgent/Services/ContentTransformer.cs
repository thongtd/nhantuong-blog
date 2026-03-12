using Markdig;

namespace DailyContentWriter.Services;

public class ContentTransformer
{
    private readonly MarkdownPipeline _pipeline;

    public ContentTransformer()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string ConvertMarkdownToHtml(string markdown)
    {
        return Markdown.ToHtml(markdown ?? string.Empty, _pipeline);
    }
}