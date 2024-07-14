using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public class EditExpense : ModifyExpense
    {
        public EditExpense(DataService dataService) : base(false, dataService)
        {
        }

        public override EventNumber GetId()
        {
            return MenuNumber.EditExpense;
        }
    }
}
