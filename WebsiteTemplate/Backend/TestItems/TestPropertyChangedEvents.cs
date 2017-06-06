using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.PropertyChangedEvents;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestPropertyChangedEvents : GetInput
    {
        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override string Description
        {
            get
            {
                return "Test Property Changed Events";
            }
        }

        public override EventNumber GetId()
        {
            return 8736;
        }

        public override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();

            result.Add(new BooleanInput("FilterItems", "Filter Items", false)
            {
                RaisePropertyChangedEvent = true
            });

            result.Add(new EnumComboBoxInput<FilterComparison>("Comparison", "Comparison", false, null, null, null, null)
            {
                Mandatory = true
            });

            result.Add(new DataSourceComboBoxInput<User>("User", "User", x => x.Id, x => x.UserName, null, null, null, null, true, false)
            {
                RaisePropertyChangedEvent = true,
            });

            result.Add(new StringInput("Email", "Email", "q10athome@gmail.com", null, false)
            {
                VisibilityConditions = new List<Condition>()
                {
                    new Condition("User", Comparison.Equals, "q10athome@gmail.com")
                }
            });

            result.Add(new ListSelectionInput("List", "List", null, null, false)
            {
                ListSource = new Dictionary<string, object>()
                {
                    {  "1", "Item 1" },
                    { "2", "Item 2" }
                }
            });

            return result;
        }
        
        public override async Task<IList<IEvent>> OnPropertyChanged(string propertyName, object propertyValue)
        {
            var result = new List<IEvent>();

            if (propertyName == "FilterItems")
            {
                var change = Convert.ToBoolean(propertyValue.ToString());
                if (change)
                {
                    var combo = InputFields.Where(i => i.InputName == "Comparison").Single() as EnumComboBoxInput<FilterComparison>;
                    
                    combo.UpdateList(x => x.Key == FilterComparison.Contains || x.Key == FilterComparison.NotEquals, null, false);

                    var list = combo.ListItems;
                    result.Add(new UpdateComboBoxSource("Comparison", list));


                    var listInput = InputFields.Where(i => i.InputName == "List").Single() as ListSelectionInput;
                    listInput.ListSource = new Dictionary<string, object>()
                    {
                        { "x", "ITem XXX" }
                    };
                    var tmp = listInput.ListSource;
                    result.Add(new UpdateComboBoxSource("List", tmp));
                }
            }
            else if (propertyName == "User")
            {
                var userId = propertyValue?.ToString();
                if (userId == "a444fd59-1ed5-49f4-80be-4c6c1dae6aa2")
                {
                    result.Add(new UpdateInputVisibility("Email", true));
                }
                else
                {
                    result.Add(new UpdateInputVisibility("Email", false));
                }
            }

            return result;
        }

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
                var user = GetDataSourceValue<User>("User");
                return new List<IEvent>()
                {
                    new ShowMessage("Done"),
                    new CancelInputDialog(),
                };
            }

            return null;
        }
    }

    public enum FilterComparison
    {
        Equals = 0,
        GreaterThan = 1,
        GreaterThanOrEquals = 2,
        LessThan = 3,
        LessThanOrEquals = 4,
        NotEquals = 5,
        Contains = 6
    }
}