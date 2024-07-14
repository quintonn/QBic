using NHibernate;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public class ViewExpenses : ViewForInput
    {
        public override string Description => "View Expenses";

        public override bool AllowInMenu => false;

        private DataService DataService { get; set; }

        public ViewExpenses(DataService dataService)
        {
            DataService = dataService;
        }

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
        // TODO: The get data and data count are not long working.
        //       remove them from ViewForInput
        //       have to instead set the defaults maybe inside the constructor of ViewForInput since we're only allow 1 page
        //public override IEnumerable GetData(GetDataSettings settings)
        //{
        //    using (var session = DataService.OpenSession())
        //    {
        //        var data = CreateQuery(session, settings).List<Expense>();
                
        //        return data.Select(x => new
        //        {
        //            x.Name,
        //            Category = x.Category.ToString(),
        //            Type = x.ExpenseType.ToString(),
        //            x.Quantity,
        //            x.Amount,
        //            x.Frequency,
        //            x.Id,
        //            x.StartMonth,
        //            x.EndMonth,
        //            x.RollOutPeriod,
        //        }).ToList();
        //    }
        //}

        //public override int GetDataCount(GetDataSettings settings)
        //{
        //    using (var session = DataService.OpenSession())
        //    {
        //        var result = CreateQuery(session, settings).RowCount();
        //        return result;
        //    }
        //}

        public virtual IQueryOver<Expense> CreateQuery(ISession session, GetDataSettings settings)
        {
            var query = session.QueryOver<Expense>()
                               .Where(q => q.Department.Id == settings.ViewData)
                               .OrderBy(x => x.Name).Asc;

            return query;
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
