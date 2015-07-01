using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ShowHideColumnSetting : ColumnSetting
    {
        public ColumnDisplayType Display { get; set; }

        public string OtherColumnToCheck { get; set; }

        public string OtherColumnValue { get; set; }

        public override int ColumnSettingType
        {
            get
            {
                return 0;// TODO: Make enum
            }
        }
    }
}