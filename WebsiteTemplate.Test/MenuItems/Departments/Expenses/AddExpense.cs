using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public class AddExpense : ModifyExpense
    {
        public AddExpense(DataService dataService) : base(true, dataService)
        {
        }

        public override EventNumber GetId()
        {
            return MenuNumber.AddExpense;
        }
    }
}
