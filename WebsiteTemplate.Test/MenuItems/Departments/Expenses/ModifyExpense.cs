using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public abstract class ModifyExpense : GetInput
    {
        private bool IsNew { get; set; }
        private int RowId { get; set; }
        private JsonHelper RowData { get; set; }

        public ModifyExpense(bool isNew, DataService dataService) : base(dataService)
        {
            IsNew = isNew;
        }

        public override string Description
        {
            get
            {
                if (IsNew)
                {
                    return "Add Expense";
                }
                return "Edit Expense";
            }
        }

        public override bool AllowInMenu => false;

        public override async Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);

            if (IsNew == false)
            {
                var rowData = data;
                if (!String.IsNullOrWhiteSpace(json.GetValue("rowData")))
                {
                    rowData = json.GetValue("rowData");
                }

                RowId = json.GetValue<int>("rowid");
                RowData = JsonHelper.Parse(rowData);
            }
            else
            {
                RowData = new JsonHelper();
                RowId = -1;
            }

            return new InitializeResult(true);
        }

        public override IList<InputField> GetInputFields()
        {
            var list = new List<InputField>();

            if (IsNew == false)
            {
                list.Add(new HiddenInput("ExpenseId", RowData?.GetValue("Id")));
            }

            list.Add(new StringInput("Name", "Name", RowData?.GetValue("Name"), null, true));
            
            list.Add(new EnumComboBoxInput<ExpenseCategory>("Category", "Category", false, null, x => x.Value, RowData?.GetValue("Category"), null)
            {
                Mandatory = true,
            });
            list.Add(new EnumComboBoxInput<ExpenseType>("Type", "Type", false, null, x => x.Value, RowData?.GetValue("Type"), null)
            {
                Mandatory = true
            });
            
            list.Add(new NumericInput<int>("Quantity", "Quantity", GetDefault("Quantity", "1"), null, true));
            list.Add(new NumericInput<double>("Amount", "Amount", GetDefault("Amount", "0"), null, true));

            list.Add(new EnumComboBoxInput<ExpenseFrequency>("Frequency", "Frequency", false, null, x => x.Value, RowData?.GetValue("Frequency"), null)
            {
                Mandatory = true,
                VisibilityConditions = new List<Condition>()
                {
                    new Condition("Category", Comparison.NotEquals, ExpenseCategory.Resource.ToString()),
                }
            });

            list.Add(new NumericInput<int>("StartMonth", "Start Month", GetDefault("StartMonth", "1"), null, true));
            list.Add(new NumericInput<int>("EndMonth", "End Month", GetDefault("EndMonth", "60"), null, true)
            {
                VisibilityConditions = new List<Condition>()
                {
                    new Condition("Frequency", Comparison.NotEquals, ExpenseFrequency.OnceOff.ToString()),
                }
            });

            list.Add(new NumericInput<int>("RollOutPeriod", "Roll Out Period", GetDefault("RollOutPeriod"), null, true)
            {
                VisibilityConditions = new List<Condition>()
                {
                    new Condition("Category", Comparison.NotEquals, ExpenseCategory.ResourceExpense.ToString()),
                    new Condition("Type", Comparison.Equals, ExpenseType.Opex.ToString()),
                },
            });


            list.Add(new HiddenInput("rowId", RowId));

            return list;
        }

        private string GetDefault(string fieldName, string defaultValue = "")
        {
            var result = RowData?.GetValue(fieldName);
            if (String.IsNullOrWhiteSpace(result))
            {
                result = defaultValue;
            }
            return result;
        }

        //public override async Task<IList<IEvent>> OnPropertyChanged(string propertyName, object propertyValue)
        //{
        //    if (propertyName == "Category")
        //    {
        //        //if (propertyValue?.ToString() == ExpenseCategory.Resource.ToString())
        //        //{
        //        //    var items = new Dictionary<string, object>();
        //        //    items.Add(ExpenseType.Capex.ToString(), "Capex");
        //        //    //return new List<IEvent>() { new UpdateComboBoxSource("Type", items) };
        //        //    return new List<IEvent>() { new UpdateInput("Category", ExpenseCategory.Licensing.ToString()) };
        //        //}
        //        if (String.IsNullOrWhiteSpace(propertyValue?.ToString()))
        //        {
        //            return new List<IEvent>() { new UpdateInputVisibility("RollOutPeriod", false) };
        //        }
        //    }

        //    return await base.OnPropertyChanged(propertyName, propertyValue);
        //}

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                };
            }
            else if (actionNumber == 0)
            {
                //InputData["ExpenseId"] = outcome.Id;
                //InputData["OutcomeName"] = outcome.Name;
                //InputData.Add("Id", outcome.Id);
                InputData["Category"] = GetValue<ExpenseCategory>("Category").ToString();
                InputData["Type"] = GetValue<ExpenseType>("Type").ToString();
                InputData["Frequency"] = GetValue<ExpenseFrequency>("Frequency").ToString();

                return new List<IEvent>()
                {
                    new UpdateInputView(InputViewUpdateType.AddOrUpdate),
                    new CancelInputDialog()
                };
            }

            return null;
        }
    }
}