using System.Text;
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

    public string InsertImagesToHtml(string htmlBody, params string[] imageUrls)
    {
        var sb = new StringBuilder();

        foreach (var imageUrl in imageUrls.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var safeUrl = System.Net.WebUtility.HtmlEncode(imageUrl);
            sb.AppendLine($@"<figure class=""article-image""><img src=""{safeUrl}"" alt=""image"" /></figure>");
        }

        sb.AppendLine(htmlBody);
        return sb.ToString();
    }
}