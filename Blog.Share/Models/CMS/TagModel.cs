using MicroBase.Dashboard.Share.Models.RoboForm.UI;
using MicroBase.Share.Models;

namespace Blog.Share.Models.CMS
{
    public class TagModel : BaseModel
    {
        [RoboText(LabelText = "Từ khóa", Name = "Keyword", MaxLength = 255, IsRequired = true, Cols = 12, Order = 1)]
        public string Keyword { get; set; }

        [RoboText(Type = RoboTextType.MultiText, LabelText = "Mô tả", Name = "Description", MaxLength = 512, Cols = 12, Order = 4)]
        public string Description { get; set; }

        [RoboCheckbox(LabelText = "Hiển thị", Name = "Enabled", Order = 6)]
        public bool Enabled { get; set; }
    }

    public class TagCmsResponse : TagModel
    {
        public int Order { get; set; }
    }
}