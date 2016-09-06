using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestView : ShowView
    {
        public override string Description
        {
            get
            {
                return "Test View";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");

            columnConfig.AddLinkColumn("", "Name", "Test Pdf", 555);
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            return new List<object>()
            {
                new
                {
                    Name=  "Test",
                }
            };
        }

        public override int GetDataCount(string data, string filter)
        {
            return 1;
        }

        public override EventNumber GetId()
        {
            return new EventNumber(554);
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>();
        }
    }
}