using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems.ViewDetail;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments
{
    public class DepartmentDetailsSection : ViewDetailSection
    {
        public override string Description => "Department Details";

        public override IList<EventNumber> GetDetailComponentIds(string requestData)
        {
            return new List<EventNumber>()
            {
                MenuNumber.ExpenseDetailsComponent
            };
        }

        public override EventNumber EventId => MenuNumber.DepartmentDetailsSection;
    }
}
