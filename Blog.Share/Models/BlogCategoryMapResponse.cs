using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class BlogCategoryMapResponse : BaseModel
    {
        public int Code { get; set; }

        public string Name { get; set; }
    }
}
