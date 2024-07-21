using System.Collections.Generic;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public class ViewExpenses : ViewForInput
    {
        public override string Description => "View Expenses";

        public override bool AllowInMenu => false;

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Category", "Category");
            columnConfig.AddStringColumn("Type", "Type");
            columnConfig.AddStringColumn("Quantity", "Quantity");
            columnConfig.AddStringColumn("Amount", "Amount");
            columnConfig.AddStringColumn("Frequency", "Frequency");

            columnConfig.AddLinkColumn("", "Id", "Edit", MenuNumber.EditExpense);
            columnConfig.AddButtonColumn("", "Id", "X", new UserConfirmation("Delete selected item?")
            {
                OnConfirmationUIAction = MenuNumber.DeleteExpense
            });
        }

        public override EventNumber GetId()
        {
            return MenuNumber.ViewExpenses;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>()
            {
                new MenuItem("Add", MenuNumber.AddExpense)
            };
        }
    }
}
