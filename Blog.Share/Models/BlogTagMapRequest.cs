using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class BlogTagMapRequest : BaseModel
    {
        public int TagId { get; set; }

        public string Name { get; set; }
    }
}