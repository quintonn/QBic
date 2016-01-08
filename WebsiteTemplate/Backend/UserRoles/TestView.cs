using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class TestView : ViewForInput
    {
        public override string Description
        {
            get
            {
                return "xxx";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "name");
            columnConfig.AddStringColumn("Age", "age");
            columnConfig.AddLinkColumn("", "", "name", "edit", 777);
        }

        public override IEnumerable GetData(string data)
        {
            return new List<object>()
            {
                new
                {
                    name = "Steve",
                    age = 10
                },
                new
                {
                    name = "Bob",
                    age = 20
                }
            };
        }

        public override int GetId()
        {
            return 9854;
        }

        public override IList<MenuItem> GetViewMenu()
        {
            return new List<MenuItem>()
            {
                new MenuItem("Add",777)
            };
        }
    }
}