using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Menus.InputItems
{
    public class ViewInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.View;
            }
        }

        public ViewForInput ViewForInput { get; set; }

        public ViewInput(string name, string label, ViewForInput viewForInput, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
            ViewForInput = viewForInput;
        }

        public override object GetValue(JToken jsonToken)
        {
            if (jsonToken is JObject)
            {
                var data = (jsonToken as JObject).GetValue("data") as JObject;
                var viewData = data.GetValue("ViewData") as JArray;
                return viewData.ToList();
            }
            throw new Exception("Unable to call GetValue for ViewInput with value:\n"+jsonToken?.ToString());
        }
    }
}