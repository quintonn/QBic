using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class DateInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.Date;
            }
        }

        public DateInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue == null || String.IsNullOrWhiteSpace(defaultValue.ToString()) ? "" : (DateTime.Parse(defaultValue.ToString())).ToString("yyyy-MM-dd"), tabName, mandatory)
        {
        }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}