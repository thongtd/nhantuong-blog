using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MicroBase.Entity;

namespace Blog.Entity.Entities
{
    [Table("blogs")]
    public class Blog : BaseEntity<int>
    {
        [Column("name")]
        [Required, MaxLength(255)]
        public string Name { get; set; }
        
        [Column("slug")]
        [MaxLength(255)]
        public string? Slug { get; set; }
        
        [Column("thumbnail")]
        [MaxLength(5000)]
        public string? Thumbnail { get; set; }
        
        [Column("sub_content")]
        [MaxLength(512)]
        public string? SubContent { get; set; }
        
        [Column("body_content")]
        [Required]
        public string BodyContent { get; set; }
        
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
        [Required]
        public bool Enabled { get; set; }
        
        [Column("view")]
        [Required]
        public decimal View { get; set; }
        
        [Column("hot_news")]
        [Required]
        public bool HotNews { get; set; }

        public virtual Collection<BlogCategoryMap> BlogCategoryMaps { get; set; }

        public virtual Collection<BlogTagMap> BlogTagMaps { get; set; }
    }
}