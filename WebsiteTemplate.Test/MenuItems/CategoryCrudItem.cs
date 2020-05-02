using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.MenuItems
{
    public class CategoryCrudItem : BasicCrudMenuItem<Category>
    {
        public CategoryCrudItem()
        {
        }

        public override bool AllowInMenu => true;

        public override string GetBaseItemName()
        {
            return "Category";
        }

        public override EventNumber GetBaseMenuId()
        {
            return 6530;
        }

        public override Dictionary<string, string> GetColumnsToShowInView()
        {
            var res = new Dictionary<string, string>();
            res.Add("Name", "Name");
            res.Add("Description", "Description");
            return res;
        }

        public override Dictionary<string, string> GetInputProperties()
        {
            var res = new Dictionary<string, string>();
            res.Add("Name", "Name");
            res.Add("Description", "Description");
            return res;
        }
    }
}