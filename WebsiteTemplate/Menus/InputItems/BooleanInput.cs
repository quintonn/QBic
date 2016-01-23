using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class BooleanInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Boolean;
            }
        }

        public BooleanInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
        }

        public override object GetValue(JToken jsonToken)
        {
            var boolString = "false";
            if (jsonToken != null)
            {
                boolString = jsonToken.ToString();
            }

            return Boolean.Parse(boolString);
        }
    }
}