namespace Blog.Share.Models.Weddings
{
    public class BlogByCategoryResponse
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Thumbnail { get; set; }

        public List<BlogResponse> SubItems { get; set; }
    }
}
