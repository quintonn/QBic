using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    public class FileInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.File;
            }
        }
        public FileInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
        }

        public override object GetValue(JToken jsonToken)
        {
            if (jsonToken == null)
            {
                return new byte[0];
            }
            var fileData = jsonToken.ToString();
            var binData = Convert.FromBase64String(fileData);
            return binData;
        }
    }
}