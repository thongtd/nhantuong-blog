using MicroBase.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Entity.Entities
{
    [Table("blog_categories")]
    public class BlogCategory : BaseEntity<int>
    {
        [Column("name")]
        [Required, MaxLength(255)]
        public string Name { get; set; }
        
        [Column("slug")]
        [MaxLength(255)]
        public string? Slug { get; set; }
        
        [Column("parent_id")]
        public int? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual BlogCategory ParentCategory { get; set; }
        
        [Column("thumbnail")]
        [MaxLength(5000)]
        public string? Thumbnail { get; set; }
        
        [Column("order")]
        [Required]
        public int? Order { get; set; }
        
        [Column("level")]
        [MaxLength(255)]
        public string? Level { get; set; }
        
        [Column("title")]
        [MaxLength(255)]
        public string? Title { get; set; }
        
        [Column("description")]
        [MaxLength(512)]
        public string? Description { get; set; }
        
        [Column("keyword")]
        [MaxLength(512)]
        public string? Keyword { get; set; }
        
        [Column("enabled")]
        public bool Enabled { get; set; }

        [Column("sitemap_indexed")]
        public bool SitemapIndexed { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }
    }
}