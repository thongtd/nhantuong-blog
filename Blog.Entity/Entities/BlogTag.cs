using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MicroBase.Entity;

namespace Blog.Entity.Entities
{
    [Table("blog_tags")]
    public class BlogTag : BaseEntity<int>
    {
        [Column("keyword")]
        [Required, MaxLength(255)]
        public string Keyword { get; set; }

        [Column("nomalization_keyword")]
        [MaxLength(255)]
        public string NomalizationKeyword { get; set; }

        [Column("order")]
        public int? Order { get; set; }

        [Column("slug")]
        [MaxLength(255)]
        public string? Slug { get; set; }

        [Column("enabled")]
        public bool Enabled { get; set; }

        [Column("description")]
        [MaxLength(512)]
        public string? Description { get; set; }
    }
}
