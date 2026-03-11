using Microsoft.AspNetCore.Http;
using MicroBase.Share.Models;
using MicroBase.Dashboard.Share.Models.RoboForm.UI;

namespace Blog.Share.Models
{
    public class BlogModel : BaseModel
    {
        [RoboFileUpload(LabelText = "Ảnh", Name = "LogoFile", IsShowPreview = true, ThumbnailField = "ThumbnailUrl", Order = 1)]
        public IFormFile? LogoFile { get; set; }

        [RoboText(Name = "ThumbnailUrl", IsHidden = true)]
        public string? ThumbnailUrl { get; set; }

        [RoboText(LabelText = "Tên", Name = "Name", MaxLength = 255, IsRequired = true, Cols = 6, Order = 2)]
        public string? Name { get; set; }

        [RoboText(LabelText = "Slug", Name = "Slug", MaxLength = 255, Cols = 6, Order = 3)]
        public string? Slug { get; set; }

        [RoboSelect2(LabelText = "Thể loại", Name = "BlogCategory", Cols = 6, Order = 4, FirstOptionLabel = "Chọn thể loại", IsRequired = true, AllowMultiple = true)]
        public List<string> BlogCategory { get; set; }

        [RoboSelect2(LabelText = "Tag", Name = "Tag", Cols = 6, Order = 5, IsRequired = true, AllowMultiple = true)]
        public List<string> Tag { get; set; }

        [RoboText(Type = RoboTextType.MultiText, LabelText = "Nội dung phụ", Name = "SubContent", MaxLength = 512, Cols = 12, Order = 6)]
        public string? SubContent { get; set; }

        [RoboText(Type = RoboTextType.RichText, LabelText = "Nội dung chính", Name = "BodyContent", IsRequired = true, Cols = 12, Order = 7)]
        public string BodyContent { get; set; }

        [RoboText(LabelText = "SEO - Tiêu đề", Name = "Title", MaxLength = 255, Cols = 6, Order = 8)]
        public string? Title { get; set; }

        [RoboText(LabelText = "Số lượt xem", RegexPattern = @"^\d+$", IsReadOnly = true, RegexValue = "Giá trị không hợp lệ", Name = "View", Cols = 6, Order = 9)]
        public int? View { get; set; }

        [RoboText(Type = RoboTextType.MultiText, LabelText = "SEO - Từ khóa", Name = "Keyword", MaxLength = 255, Cols = 6, Order = 10)]
        public string? Keyword { get; set; }

        [RoboText(Type = RoboTextType.MultiText, LabelText = "SEO - Mô tả", Name = "Description", MaxLength = 512, Cols = 6, Order = 11)]
        public string? Description { get; set; }

        [RoboCheckbox(LabelText = "Nổi bật", Name = "HotNews", Order = 12, Cols = 12)]
        public bool HotNews { get; set; }

        [RoboCheckbox(LabelText = "Hiển thị", Name = "Enabled", Order = 13, Cols = 12)]
        public bool Enabled { get; set; }

        //public List<BlogCategoryOption> BlogCategoryOptions { get; set; }
    }
}