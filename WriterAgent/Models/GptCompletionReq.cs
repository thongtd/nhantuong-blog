namespace WriterAgent.Models
{
    public class GptCompletionReq
    {
        public string Model { get; set; }

        public float Temperature { get; set; }

        public List<GptMessageModel> Messages { get; set; }
    }

    public class GptMessageModel
    {
        public string Role { get; set; }

        public string Content { get; set; }
    }
}