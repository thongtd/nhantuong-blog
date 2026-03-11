namespace Blog.Share.Models
{
    public class TopBlogModel
    {
        public int BlogCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategorySlug { get; set; }

        public List<BlogResponse> Blogs { get; set; }
    }
}