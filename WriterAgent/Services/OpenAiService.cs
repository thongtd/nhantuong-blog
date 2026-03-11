using DailyContentWriter.Models;
using MicroBase.Share.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using WriterAgent.Models;

namespace DailyContentWriter.Services;

public class OpenAiService
{
    private readonly AppSettings _settings;
    private readonly HttpClient _httpClient;

    public OpenAiService(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
    {
        _settings = options.Value;
        this._httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(_settings.ChatGpt.ApiUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ChatGpt.Token);
    }

    public async Task<ArticleResult> GenerateArticleAsync(string mainContent, string seoKeywords,int noOfImg)
    {
        var request = BuildPrompt(mainContent, seoKeywords, noOfImg);
        var genContent = await _httpClient.PostRequestAsync<GptCompletionReq, GptCompletionRes>("/chat/completions", request);

        var outputText = genContent.Data.Choices[0].Message.Content;
        var article = JsonSerializer.Deserialize<ArticleResult>(outputText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (article == null)
        {
            throw new Exception("Không parse được ArticleResult từ phản hồi model.");
        }

        return article;
    }

    private static GptCompletionReq BuildPrompt(string mainContent, string seoKeywords, int noOfImg)
    {
        var userMessages = new GptMessageModel
        {
            Role = "user",
            Content = $"""
CHỦ ĐỀ:\n{mainContent}\n\nTỪ KHÓA SEO:\n{seoKeywords}\n\Số lượng ảnh:\n{noOfImg}
"""
        };

        var request = new GptCompletionReq
        {
            Model = "gpt-5-mini",
            Temperature = 1,
            Messages = new List<GptMessageModel>
            {
                new GptMessageModel
                {
                    Role = "system",
                    Content=
$$"""
Bạn là một chuyên gia Nhân Tướng học và Nhân tướng ứng dụng.

Bạn đã có nhiều năm kinh nghiệm xem tướng, luận tướng và tư vấn cải tướng cho nhiều người trong các lĩnh vực như sự nghiệp, tài lộc, hôn nhân và vận mệnh. Bạn có khả năng giải thích kiến thức nhân tướng học theo cách dễ hiểu, thực tế và mang tính ứng dụng trong đời sống hiện đại.

Nhiệm vụ của bạn:

Dựa trên CHỦ ĐỀ và TỪ KHÓA được cung cấp, hãy biên tập và phát triển một bài viết (blog) chuyên sâu, giàu giá trị cho người đọc về Nhân Tướng học.

Yêu cầu nội dung:

1. Nội dung phải mang tính chuyên môn, có chiều sâu, giải thích rõ ràng các khái niệm trong nhân tướng học.
2. Văn phong mạch lạc, tự nhiên, dễ đọc và giàu giá trị cho người đọc.
3. Nội dung cần mang tính ứng dụng thực tế, giúp người đọc hiểu rõ đặc điểm tướng và cách cải thiện vận mệnh hoặc hành vi.
4. Viết hoàn toàn bằng tiếng Việt.
5. Không sử dụng emoji.
6. Nội dung cần được ngắt nghỉ đoạn văn hợp lý.
7. Chỉ viết nội dung theo chủ đề và từ khóa cung cấp, không được hỏi lại hoặc gợi ý mở rộng chủ đề.
8. Chèn ảnh vào vị trí thích hợp với key ANH_{Số thứ tự} dựa theo số lượng anh được cung cấp.

Yêu cầu SEO:

1. Tối ưu SEO cho bài viết.
2. Sử dụng các từ khóa được cung cấp một cách tự nhiên trong:
   - tiêu đề
   - các heading
   - nội dung bài viết
3. Cấu trúc bài viết nên bao gồm:
   - Tiêu đề SEO hấp dẫn
   - Các mục nội dung chính (H2 / H3)
   - Phần tổng kết hoặc lời khuyên ứng dụng

Định dạng trả về:

Chỉ trả về JSON hợp lệ với cấu trúc sau:
{
  \"title\": \"Tiêu đề bài viết chuẩn SEO (không cố nhồi nhét từ khóa)\",
  \"markdownBody\": \"Toàn bộ nội dung bài viết ở dạng Markdown\"
}

Lưu ý:

- Nội dung bài viết phải ở dạng Markdown
- Không chèn ảnh markdown
- Không thêm markdown code block
- Không thêm giải thích ngoài JSON
"""
                },
                userMessages
            }
        };

        return request;
    }
}