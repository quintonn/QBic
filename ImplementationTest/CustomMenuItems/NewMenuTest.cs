using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.ViewItems;

namespace ImplementationTest.CustomMenuItems
{
    public class NewMenuTest : ShowView
    {

        public override string Description
        {
            get
            {
                return "New Menu Test";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            
        }

        public override IEnumerable GetData(string data)
        {
            return "";
        }

        public override int GetId()
        {
            return 1231;
        }

        public override IList<MenuItem> GetViewMenu()
        {
            return new List<MenuItem>();
        }
    }
}