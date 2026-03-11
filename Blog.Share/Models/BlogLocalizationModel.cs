using MicroBase.Share.Models;

namespace Blog.Share.Models
{
    public class BlogLocalizationModel : BaseModel
    {
       //  [RoboText(IsHidden = true, Name = "Id")]
        public override int? Id { get; set; }

       //  [RoboText(LabelText = "Tiêu đề", Name = "Name", MaxLength = 255, IsRequired = true, Cols = 4, Order = 2)]
        public string Name { get; set; }

       //  [RoboText(Type = RoboTextType.MultiText, LabelText = "Nội dung phụ", Name = "SubContent", MaxLength = 512, IsRequired = true, Cols = 12, Order = 5)]
        public string SubContent { get; set; }

       //  [RoboText(Type = RoboTextType.RichText, LabelText = "Nội dung chính", Name = "BodyContent", IsRequired = true, Cols = 12, Order = 6)]
        public string BodyContent { get; set; }

       //  [RoboText(LabelText = "SEO - Tiêu đề", Name = "Title", MaxLength = 255, Cols = 6, Order = 7)]
        public string Title { get; set; }

       //  [RoboText(Type = RoboTextType.MultiText, LabelText = "SEO - Từ khóa", Name = "Keyword", MaxLength = 255, Cols = 12, Order = 8)]
        public string Keyword { get; set; }

       //  [RoboText(Type = RoboTextType.MultiText, LabelText = "SEO - Nội dung", Name = "Description", MaxLength = 512, Cols = 12, Order = 9)]
        public string Description { get; set; }
    }
}