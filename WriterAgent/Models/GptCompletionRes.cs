using Newtonsoft.Json;

namespace WriterAgent.Models
{
    public class GptCompletionRes
    {
        public string Id { get; set; }

        public string Object { get; set; }

        public long Created { get; set; }

        public string Model { get; set; }

        public List<Choice> Choices { get; set; }

        public Usage Usage { get; set; }

        public string ServiceTier { get; set; }

        public string SystemFingerprint { get; set; }

        public Guid MessageId { get; set; }
    }

    public class Choice
    {
        public int Index { get; set; }

        public Message Message { get; set; }

        public object Logprobs { get; set; }

        public string FinishReason { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }

        public string Content { get; set; }

        public object Refusal { get; set; }

        public List<object> Annotations { get; set; }
    }

    public class Usage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
}