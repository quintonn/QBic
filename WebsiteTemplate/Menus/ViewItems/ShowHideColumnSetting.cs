using System.Collections.Generic;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ShowHideColumnSetting : ColumnSetting
    {
        public ColumnDisplayType Display { get; set; }

        public override int ColumnSettingType
        {
            get
            {
                return 0;// TODO: Make enum
            }
        }

        public List<Condition> Conditions { get; set; }

        public ShowHideColumnSetting()
        {
            Conditions = new List<Condition>();
        }
    }
}