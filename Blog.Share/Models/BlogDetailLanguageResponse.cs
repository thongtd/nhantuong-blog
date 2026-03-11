namespace Blog.Share.Models
{
    public class BlogDetailLanguageResponse
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Thumbnail { get; set; }

        public string SubContent { get; set; }

        public string BodyContent { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Keyword { get; set; }

        public DateTime? CreatedDate { get; set; }

        public List<BlogCategoryRes> BlogCategory { get; set; }
    }
}