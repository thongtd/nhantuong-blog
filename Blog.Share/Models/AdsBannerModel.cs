using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class AdsBannerModel : BaseModel
    {
       //  [RoboText(LabelText = "Name", Name = "Name", MaxLength = 255, Cols = 12, Order = 1)]
        public string Name { get; set; }

       //  [RoboFileUpload(LabelText = "Thumbnail", Name = "Thumbnail", IsShowPreview = true, ThumbnailField = "ThumbnailUrl", IsRequired = true, Order = 2)]
        [JsonIgnore]
        public IFormFile Thumbnail { get; set; }

       //  [RoboText(IsHidden = true)]
        public string ThumbnailUrl { get; set; }

       //  [RoboText(LabelText = "Action To Link", Name = "ActionToLink", MaxLength = 255, Cols = 12, Order = 3)]
        public string ActionToLink { get; set; }

       //  [RoboDropDown(LabelText = "Display Order", FirstOptionLabel = "Choose", Name = "DisplayOrder", IsRequired = true, Order = 4)]
        public string DisplayOrder { get; set; }

       //  [RoboCheckbox(LabelText = "Enabled", Name = "Enabled", Order = 5)]
        public bool Enabled { get; set; }
    }
}
