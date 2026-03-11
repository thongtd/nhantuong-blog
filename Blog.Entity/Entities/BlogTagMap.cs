using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MicroBase.Entity;

namespace Blog.Entity.Entities
{
    [Table("blog_tag_maps")]
    public class BlogTagMap : BaseEntity<int>
    {
        [Column("blog_id")]
        [Required]
        public int BlogId { get; set; }
        
        [Column("tag_id")]
        [Required]
        public int TagId { get; set; }
        
        [Column("enabled")]
        [Required]
        public bool Enabled { get; set; }

        [ForeignKey(nameof(BlogId))]
        public virtual Blog Blog { get; set; }

        [ForeignKey(nameof(TagId))]
        public virtual BlogTag BlogTag { get; set; }
    }
}
