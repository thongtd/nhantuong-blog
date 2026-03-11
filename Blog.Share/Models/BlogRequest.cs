using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class BlogRequest : BaseModel
    {
        [Required]
        public IFormFile LogoFile { get; set; }

        [MaxLength(255)]
        public string ThumbnailUrl { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        [Required]
        public List<BlogCategoryMapResponse> Category { get; set; }

        [Required, MaxLength(512)]
        public string SubContent { get; set; }

        [Required]
        public string BodyContent { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Keyword { get; set; }

        [Required]
        public string Tag { get; set; }

        public bool Enabled { get; set; }

        public bool HotNews { get; set; }

        public List<BlogCategoryOption> BlogCategoryOptions { get; set; }
    }
}
