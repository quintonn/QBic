using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
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
            var viewData = JArray.Parse(jsonToken.ToString());
            return viewData.ToList();
        }
    }
}