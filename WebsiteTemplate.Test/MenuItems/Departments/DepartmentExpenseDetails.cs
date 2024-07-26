using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus.ViewItems.ViewDetail;
using WebsiteTemplate.Test.MenuItems.Departments.Expenses;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.MenuItems.Departments
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

            //TODO: In-place editing ??
        }

        public override IEnumerable GetData(string data)
        {
            using (var session = DataService.OpenSession())
            {
                var json = JsonHelper.Parse(data);
                var id = json.GetValue("Id");
                var expenses = session.QueryOver<Expense>().Where(x => x.Department.Id == id).OrderBy(x => x.Name).Asc.List().ToList();

                var results = expenses.Select((x) => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category.ToString(),
                    Type = x.ExpenseType.ToString(),
                    Quantity = x.Quantity,
                    Amount = x.Amount,
                    Frequency = x.Frequency.ToString(),
                    StartMonth = x.StartMonth,
                    EndMonth = x.EndMonth,
                    RollOutPeriod = x.RollOutPeriod,
                });

                return results;
            }
        }
    }
}
