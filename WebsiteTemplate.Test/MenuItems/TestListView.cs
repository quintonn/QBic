using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestListView : ShowListView
    {
        public override string Description
        {
            get
            {
                return "Test List View";
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            var results = new List<object>();

            for (var i = 0; i < 20; i++)
            {
                results.Add(new
                {
                    Id = i,
                    Name = "Test Item " + i,
                    Description = "Detail item " + i,
                    LongDesc = "More detail"
                });
            }
            return results;
        }

        public override IList<string> GetDetailColumnNames()
        {
            return new List<string>()
            {
                "Description",
                "LongDesc"
            };
        }

        public override EventNumber GetDetailViewEventNumber()
        {
            return 321;
        }

        public override string GetHeadingColumnName()
        {
            return "Name";
        }

        public override EventNumber GetId()
        {
            return 320;
        }

        public override string GetIdColumnName()
        {
            return "Id";
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>();
        }
    }
}