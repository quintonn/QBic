using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments
{
    public class EditDepartment : ModifyDepartment
    {
        public EditDepartment(DataService dataService) : base(dataService, false)
        {
        }

        public override EventNumber GetId()
        {
            return MenuNumber.EditDepartment;
        }
    }
}
