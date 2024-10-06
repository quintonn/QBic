using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;
using WebsiteTemplate.Test.MenuItems.Departments.Expenses;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments
{
    public abstract class ModifyDepartment : CoreModify<Department>
    {
        public ModifyDepartment(DataService dataService, bool isNew) : base(dataService, isNew)
        {
        }

        public override string EntityName => "Department";

        public override EventNumber GetViewNumber()
        {
            return MenuNumber.ViewDepartments;
        }

        public override List<InputField> InputFields()
        {
            var result = new List<InputField>();

            result.Add(new StringInput("Name", "Name", Item?.Name, "General", true));

            List<IViewInputValue> defaultValues = null;
            if (!string.IsNullOrWhiteSpace(Item?.Id))
            {
                using var session = DataService.OpenStatelessSession();
                var expenseItems = session.QueryOver<Expense>().Where(x => x.Department.Id == Item.Id).List().ToList();
                defaultValues = expenseItems.OrderBy(x => x.Name).Select((x, index) => new ExpenseRowItem()
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
                    rowId = index, 
                }).Cast<IViewInputValue>().ToList();
            }
            
            result.Add(new ViewInput<ExpenseRowItem>("Expenses", "Expenses", new ViewExpenses(), defaultValues, "General", true));

            result.Add(new DateInput("Date", "Date", IsNew ? DateTime.Today : Item?.Date, "General", true));

            return result;
        }
        public override async Task<IList<IEvent>> PerformModify(bool isNew, string id, ISession session)
        {
            Department dbItem;
            if (isNew)
            {
                dbItem = new Department();
            }
            else
            {
                dbItem = session.Get<Department>(id);
            }

            dbItem.Name = GetValue("Name");
            dbItem.Date = GetValue<DateTime>("Date");

            session.SaveOrUpdate(dbItem);

            var expenseInfo = GetValue<List<ExpenseRowItem>>("Expenses") ?? new List<ExpenseRowItem>();

            if (expenseInfo.Count == 0)
            {
                return new List<IEvent>()
                    {
                        new ShowMessage("Expenses are mandatory and must be provided.")
                    };
            }

            var existingExpenseItems = session.QueryOver<Expense>().Where(x => x.Department.Id == id).List().ToList();
            var existingExpenseIds = existingExpenseItems.Select(x => x.Id).ToList();

            foreach (var item in expenseInfo)
            {
                Expense dbExpense;
                if (!string.IsNullOrWhiteSpace(item.Id))
                {
                    dbExpense = session.Get<Expense>(item.Id);
                    existingExpenseIds.Remove(item.Id);
                }
                else
                {
                    dbExpense = new Expense();
                }

                dbExpense.Name = item.Name;
                dbExpense.Category = Enum.Parse<ExpenseCategory>(item.Category);
                dbExpense.ExpenseType = Enum.Parse<ExpenseType>(item.Type);
                dbExpense.Frequency = Enum.Parse<ExpenseFrequency>(item.Frequency);
                dbExpense.StartMonth = item.StartMonth;
                dbExpense.EndMonth = item.EndMonth;
                dbExpense.RollOutPeriod = item.RollOutPeriod;
                dbExpense.Quantity = item.Quantity;
                dbExpense.Amount = item.Amount;
                dbExpense.Department = dbItem; // Assuming dbItem is defined elsewhere

                DataService.SaveOrUpdate(session, dbExpense);
            }

            foreach (var unmatchedExpense in existingExpenseIds)
            {
                var itemToDelete = session.Get<Expense>(unmatchedExpense);
                DataService.TryDelete(session, itemToDelete);
            }


            return null;
        }
    }
}
