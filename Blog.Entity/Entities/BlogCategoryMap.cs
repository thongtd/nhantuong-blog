using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MicroBase.Entity;

namespace Blog.Entity.Entities
{
    [Table("blog_category_maps")]
    public class BlogCategoryMap : BaseEntity<int>
    {
        [Column("blog_id")]
        [Required]
        public int BlogId { get; set; }

        [Column("blog_category_id")]
        [Required]
        public int BlogCategoryId { get; set; }

        [Column("enabled")]
        [Required]
        public bool Enabled { get; set; }

        [ForeignKey(nameof(BlogId))]
        public virtual Blog Blog { get; set; }

        [ForeignKey(nameof(BlogCategoryId))]
        public virtual BlogCategory BlogCategory { get; set; }
    }
}