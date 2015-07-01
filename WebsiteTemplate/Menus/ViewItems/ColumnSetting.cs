using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ColumnSetting
    {
        public abstract int ColumnSettingType { get; } // TODO: Make enum
    }
}