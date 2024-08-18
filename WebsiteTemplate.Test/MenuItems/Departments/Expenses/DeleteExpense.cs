using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public class DeleteExpense : DoSomething
    {
        public DeleteExpense(DataService dataService) : base(dataService)
        {
        }

        public override bool AllowInMenu => false;

        public override string Description => "Delete Expense";

        public override EventNumber GetId()
        {
            return MenuNumber.DeleteExpense;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            if (this.Parameters.Contains("_DELETE_"))
            {
                var id = GetValue("Id");
                using var session = DataService.OpenSession();

                var dbItem = session.Get<Expense>(id);
                session.Delete(dbItem);
                session.Flush();
                
                return new List<IEvent>()
                {
                    new ShowMessage("Item successfully deleted"),
                };
            }
            return new List<IEvent>()
            {
                new UpdateInputView(InputViewUpdateType.Delete)
            };
        }
    }
}