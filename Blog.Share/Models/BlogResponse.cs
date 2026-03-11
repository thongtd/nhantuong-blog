using MicroBase.Repository.Models.SqlModels;
using MicroBase.Share.Attributes;
using MicroBase.Share.Models.Base;

namespace Blog.Share.Models
{
    public class BlogDetailsResponse
    {
        public int Order { get; set; }

        public int? Id { get; set; }

        public string Name { get; set; }

        public string NameVI { get; set; }

        public string Slug { get; set; }

        public string Link { get; set; }

        public string Thumbnail { get; set; }

        public string SubContent { get; set; }

        public string BodyContent { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Keyword { get; set; }

        public bool HotNews { get; set; }

        public string CategoryName { get; set; }

        public decimal View { get; set; }

        public bool Enabled { get; set; }

        public DateTime? CreatedDate { get; set; }

        public List<BlogCategoryRes> BlogCategory { get; set; }

        public List<BlogTagRes> BlogTag { get; set; }
    }

    public class BlogResponse
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public decimal View { get; set; }

        public string Slug { get; set; }

        public string Thumbnail { get; set; }

        public string BodyContent { get; set; }

        public string SubContent { get; set; }

        public string Author { get; set; }
        
        public string Title { get; set; }

        public string Description { get; set; }

        public string Keyword { get; set; }

        public DateTime? CreatedDate { get; set; }

        public List<BlogCategoryRes> BlogCategory { get; set; }

        public List<BlogTagRes> BlogTag { get; set; }
    }

    public class BlogCategoryRes
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }
    }

    public class BlogTagRes
    {
        public int Id { get; set; }

        public string Keyword { get; set; }

        public string Slug { get; set; }
        
        public string NomalizationKeyword { get; set; }
    }

    public class GroupModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }

        public string Thumbnail { get; set; }
    }

    public class GroupBlogModel
    {
        public GroupModel Group { get; set; }

        public TPaging<TopBlogSqlModel> Blogs { get; set; }
    }

    public class TopBlogSqlModel : BaseSqlPagingRes
    {
        [CustomDataSet("blog_category_id")]
        public int BlogCategoryId { get; set; }

        [CustomDataSet("category_name")]
        public string CategoryName { get; set; }

        [CustomDataSet("category_slug")]
        public string CategorySlug { get; set; }

        [CustomDataSet("id")]
        public int Id { get; set; }

        [CustomDataSet("name")]
        public string Name { get; set; }

        [CustomDataSet("slug")]
        public string Slug { get; set; }

        [CustomDataSet("thumbnail")]
        public string Thumbnail { get; set; }

        [CustomDataSet("sub_content")]
        public string SubContent { get; set; }

        [CustomDataSet("title")]
        public string SeoTitle { get; set; }

        [CustomDataSet("description")]
        public string SeoDescription { get; set; }

        [CustomDataSet("keyword")]
        public string SeoKeyword { get; set; }
    }
}