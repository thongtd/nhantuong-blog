using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class TagsResponse : BaseModel
    {
        public string Keyword { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }
    }
}
