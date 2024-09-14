using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus.ViewItems.ViewDetail;
using WebsiteTemplate.Models;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.MenuItems.Departments.Details
{
    public class DepartmentExpenseDetails : ViewDetailComponent
    {
        private readonly DataService DataService;

        public DepartmentExpenseDetails(DataService dataService)
        {
            DataService = dataService;
        }

        public override EventNumber EventId => MenuNumber.ExpenseDetailsComponent;

        public override string Description => "Department Expenses";

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Category", "Category");
            columnConfig.AddStringColumn("Type", "Type");
            columnConfig.AddStringColumn("Quantity", "Quantity");
            columnConfig.AddStringColumn("Amount", "Amount");
            columnConfig.AddStringColumn("Frequency", "Frequency");

            columnConfig.AddLinkColumn("", "Id", "Edit", MenuNumber.EditExpense, null, "_EDIT_");
            columnConfig.AddButtonColumn("", "Id", "X", new UserConfirmation("Delete selected item?")
            {
                OnConfirmationUIAction = MenuNumber.DeleteExpense,
            }, null, "_DELETE_");

            //TODO: In-place editing ??  

            // todo: instead of in-place editing which will require more code, i can set the params above with a flag to tell those other items to process 
            //       input differently.
            //       I.E. instead of calling UpdateInputView, it can actually edit/delete the expense.
            //       Try this.
        }

        private string _ID { get; set; }
        public override IEnumerable GetData(string data)
        {
            using (var session = DataService.OpenSession())
            {
                var json = JsonHelper.Parse(data);
                _ID = json.GetValue("Id");
                var expenses = session.QueryOver<Expense>().Where(x => x.Department.Id == _ID).OrderBy(x => x.Name).Asc.List().ToList();

                var results = expenses.Select((x) => new
                {
                    x.Id,
                    x.Name,
                    Category = x.Category.ToString(),
                    Type = x.ExpenseType.ToString(),
                    x.Quantity,
                    x.Amount,
                    Frequency = x.Frequency.ToString(),
                    x.StartMonth,
                    x.EndMonth,
                    x.RollOutPeriod,
                });

                return results;
            }
        }

        public override Dictionary<string, string> DataForGettingMenu
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Id" , _ID },
                };
            }
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var paramItems = new
            {
                _ADD_ = true,
                Id = dataForMenu["Id"],
            };
            return new List<MenuItem>()
            {
                new MenuItem("Add", MenuNumber.AddExpense, JsonHelper.SerializeObject(paramItems))
            };
        }
    }
}
