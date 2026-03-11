using MicroBase.Share.Models;
using MicroBase.Dashboard.Share.Models.RoboForm.UI;
using Microsoft.AspNetCore.Http;

namespace Blog.Share.Models
{
    public class BlogCategoryModel : BaseModel
    {
        [RoboFileUpload(LabelText = "Ảnh", Name = "LogoFile", IsShowPreview = true, ThumbnailField = "ThumbnailUrl", Order = 1)]
        public IFormFile? LogoFile { get; set; }

        [RoboText(Name = "ThumbnailUrl", IsHidden = true)]
        public string? ThumbnailUrl { get; set; }

        [RoboText(LabelText = "Tên nhóm", Name = "Name", MaxLength = 255, IsRequired = true, Cols = 6, Order = 2)]
        public string Name { get; set; }

        [RoboText(LabelText = "Slug", Name = "Slug", MaxLength = 255, IsRequired = false, Cols = 6, Order = 3)]
        public string? Slug { get; set; }

        [RoboText(LabelText = "SEO - Tiêu đề", Name = "MetaTitle", MaxLength = 255, Cols = 12, Order = 4)]
        public string? MetaTitle { get; set; }

        [RoboText(Type = RoboTextType.MultiText, LabelText = "SEO - Từ khóa", Name = "MetaKeyword", MaxLength = 255, Cols = 12, Order = 5)]
        public string? MetaKeyword { get; set; }

        [RoboText(Type = RoboTextType.MultiText, LabelText = "SEO - Mô tả", Name = "MetaDescription", MaxLength = 512, Cols = 12, Order = 6)]
        public string? MetaDescription { get; set; }

        [RoboCheckbox(LabelText = "Hiển thị", Name = "Enabled", Order = 7)]
        public bool Enabled { get; set; }
    }
}