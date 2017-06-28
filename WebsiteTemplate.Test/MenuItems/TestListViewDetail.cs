using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestListViewDetail : ShowDetailView
    {
        public override string Description
        {
            get
            {
                return "Test List View Detail";
            }
        }

        public override bool DetailIsHtml
        {
            get
            {
                return false;
                //return true;
            }
        }

        public override IList<string> GetDetailItems()
        {
            var result = new List<string>();

            var html = "<div class='w3-padding w3-section w3-pale-blue w3-border-blue'>" +
                            "<span>This is a test</span>" +
                            "<br/>" +
                            "<span>More data</span>" +
                        "</div>";
            if (DetailIsHtml)
            {
                result.Add(html);
            }
            else
            {
                result.Add("This is a test");
                result.Add("More data");
                result.Add("Bla bla bla");
            }

            return result;
        }

        public override EventNumber GetId()
        {
            return 321;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var result = new List<MenuItem>();

            result.Add(new MenuItem("Back", 320));

            return result;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
    }
}