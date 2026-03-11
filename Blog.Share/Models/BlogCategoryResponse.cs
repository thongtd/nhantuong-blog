using MicroBase.Share.Models;
using MicroBase.Share.Models.Base;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Blog.Share.Models
{
    public class BlogCategoryResponse : BaseModel
    {
        public string Name { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Keyword { get; set; }

        public bool Enabled { get; set; }

        public NameValueModel<int?> ParentCategory { get; set; }

        public string Thumbnail { get; set; }

        public int Order { get; set; }

        public string Level { get; set; }

        public int BlogCount { get; set; }
    }

    public class BlogCategoryRequest : BlogCategoryResponse
    {
        public List<SelectListItem> BlogCategoryOptions { get; set; }

        public int? ParentId { get; set; }
    }
}