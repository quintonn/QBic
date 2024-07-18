using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Cfg;
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

            object defaultValues = null;
            if (!string.IsNullOrWhiteSpace(Item?.Id))
            {
                using var session = DataService.OpenStatelessSession();
                var expenseItems = session.QueryOver<Expense>().Where(x => x.Department.Id == Item.Id).List().ToList();
                defaultValues = expenseItems.OrderBy(x => x.Id).Select((x, index) => new
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
                    rowId = index, // TODO: Need a way to auto add rowIds, or force user to set these (auto will be better);
                }).ToList();
            }
            
            result.Add(new ViewInput("Expenses", "Expenses", new ViewExpenses(DataService), defaultValues, "General", true));

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

            session.SaveOrUpdate(dbItem);

            var expenseInfo = GetValue<List<JToken>>("Expenses") ?? new List<JToken>();

            if (expenseInfo.Count == 0)
            {
                return new List<IEvent>()
                    {
                        new ShowMessage("Expenses are mandatory and must be provided.")
                    };
            }

            var existingExpenseItems = session.QueryOver<Expense>().Where(x => x.Department.Id == id).List().ToList();
            var existingExpenseIds = existingExpenseItems.Select(x => x.Id).ToList();

            foreach (JObject item in expenseInfo)
            {
                var name = item.GetValue("Name")?.ToString();
                var category = item.GetValue("Category")?.ToObject<ExpenseCategory>();
                var type = item.GetValue("Type")?.ToObject<ExpenseType>();
                var qty = item.GetValue("Quantity")?.ToObject<int>();
                var amount = item.GetValue("Amount")?.ToObject<double>();
                var frequency = item.GetValue("Frequency")?.ToObject<ExpenseFrequency>();
                var startMonth = item.GetValue("StartMonth")?.ToObject<int>();
                var endMonth = item.GetValue("EndMonth")?.ToObject<int>();
                var rollOutPeriod = item.GetValue("RollOutPeriod")?.ToObject<int>();
                var expenseId = item.GetValue("ExpenseId")?.ToString();

                Expense dbExpense;
                if (!string.IsNullOrWhiteSpace(expenseId))
                {
                    dbExpense = session.Get<Expense>(expenseId);

                    existingExpenseIds.Remove(expenseId);
                }
                else
                {
                    dbExpense = new Expense();
                }

                dbExpense.Name = name;
                dbExpense.Category = category ?? ExpenseCategory.Other;
                dbExpense.ExpenseType = type ?? ExpenseType.Capex;
                dbExpense.StartMonth = startMonth ?? 0;
                dbExpense.EndMonth = endMonth ?? 0;
                dbExpense.RollOutPeriod = rollOutPeriod ?? 0;
                dbExpense.Quantity = qty ?? 0;
                dbExpense.Amount = amount ?? 0;

                dbExpense.Department = dbItem;

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
