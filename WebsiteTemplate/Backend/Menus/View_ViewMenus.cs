using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.Menus
{
    public class View_ViewMenus : ShowView
    {
        public override string Description
        {
            get
            {
                return "View-Menus";
            }
        }

        public override IList<MenuItem> ViewMenu
        {
            get
            {
                return new List<MenuItem>();
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable GetData(string data)
        {
            throw new NotImplementedException();
        }

        public override Type GetDataType()
        {
            return null;
        }

        public override EventNumber GetId()
        {
            return EventNumber.View_ViewMenus;
        }
    }
}