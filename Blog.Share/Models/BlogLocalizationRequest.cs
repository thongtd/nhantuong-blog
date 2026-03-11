using System.ComponentModel.DataAnnotations;
using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class BlogLocalizationRequest : BaseModel
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }

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
    }
}
