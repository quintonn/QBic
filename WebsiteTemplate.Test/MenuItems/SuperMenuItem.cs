using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class SuperMenuItem : BasicCrudMenuItem<SuperCause>
    {
        public override string GetBaseItemName()
        {
            return "Super Cause";
        }

        public override EventNumber GetBaseMenuId()
        {
            return 123;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override Dictionary<string, string> GetColumnsToShowInView()
        {
            var res = new Dictionary<string, string>();
            res.Add("SuperName", "Name");
            res.Add("SuperOk", "Ok");
            res.Add("LongName", "Long Name");
            res.Add("SuperDate", "Date");
            //res.Add("SuperChild", "Child");
            return res;
        }

        public override Dictionary<string, string> GetInputProperties()
        {
            var res = new Dictionary<string, string>();
            res.Add("SuperName", "Name");
            res.Add("SuperOk", "Ok");
            res.Add("LongName", "Long Name");
            res.Add("SuperDate", "Date");
            //res.Add("SuperChild", "Child");
            return res;
        }
    }
}