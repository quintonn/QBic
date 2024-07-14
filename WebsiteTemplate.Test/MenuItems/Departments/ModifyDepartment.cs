using Newtonsoft.Json.Linq;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
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

            result.Add(new ViewInput("Expenses", "Expenses", new ViewExpenses(DataService), Item?.Id, "General", true));

            //result.Add(new LabelInput("txtStaff", "resource/staff expenses", "", "Staff"));

            //result.Add(new LabelInput("txtStaffCosts", "resource/staff costs", "", "Staff Costs"));

            return result;
        }
        public override async Task<IList<IEvent>> PerformModify(bool isNew, string id, ISession session)
        {
            var expenseInfo = GetValue<List<JToken>>("Expenses") ?? new List<JToken>();
            return null;
        }
    }
}
