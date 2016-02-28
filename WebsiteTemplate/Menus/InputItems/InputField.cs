using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using WebsiteTemplate.Menus.ViewItems;
namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class InputField
    {
        public abstract InputType InputType { get; }

        public string InputName { get; set; }

        public string InputLabel { get; set; }

        public object DefaultValue { get; set; }

        public string TabName { get; set; }

        public List<Condition> VisibilityConditions { get; set; }

        public bool Mandatory { get; set; }

        public List<Condition> MandatoryConditions { get; set; }

        public bool RaisePropertyChangedEvent { get; set; }

        public InputField(string name, string label, object defaultValue, string tabName, bool mandatory)
        {
            InputName = name;
            InputLabel = label;
            DefaultValue = defaultValue;
            TabName = tabName;
            Mandatory = mandatory;

            VisibilityConditions = new List<Condition>();
            MandatoryConditions = new List<Condition>();
        }

        public abstract object GetValue(JToken jsonToken);
    }
}