using NHibernate;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.MenuItems
{
    public class StudentCrudMenuItem : BasicCrudMenuItem<Student>
    {
        public override bool AllowInMenu => true;

        public override string UniquePropertyName => "Name"; // Will prevent duplicate items using Description as check

        public override string GetBaseItemName()
        {
            return "Student";
        }

        public override EventNumber GetBaseMenuId()
        {
            return 56312;
        }

        public override Dictionary<string, string> GetColumnsToShowInView()
        {
            return new Dictionary<string, string>()
            {
                { "Name", "Name" },
                { "Class.Name", "Class" }
            };
        }

        public override Dictionary<string, string> GetInputProperties()
        {
            return new Dictionary<string, string>()
            {
                { "Name", "Name" },
                { "Class", "Class" }
            };
        }

        public override IQueryOver<Student> OrderQuery(IQueryOver<Student, Student> query)
        {
            return query.OrderBy(x => x.Name).Asc;
        }
    }
}