using System.Collections.Generic;
using NHibernate;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class CauseChildMenuItem : BasicCrudMenuItem<CauseChild>
    {
        public override string GetBaseItemName()
        {
            return "Cause Child";
        }

        public override EventNumber GetBaseMenuId()
        {
            return 131;
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;// return true;
            }
        }

        public override Dictionary<string, string> GetColumnsToShowInView()
        {
            var res = new Dictionary<string, string>();
            res.Add("ChildName", "Name");
            res.Add("SomeInt", "Number");
            res.Add("ChildTypeDescription", "Child Type");
            return res;
        }

        public override Dictionary<string, string> GetInputProperties()
        {
            var res = new Dictionary<string, string>();
            res.Add("ChildName", "Name");
            res.Add("SomeInt", "Number");
            res.Add("ChildType", "Child Type");
            return res;
        }

        public override IQueryOver<CauseChild> OrderQuery(IQueryOver<CauseChild, CauseChild> query)
        {
            return query.OrderBy(x => x.ChildName).Asc;
        }
    }
}