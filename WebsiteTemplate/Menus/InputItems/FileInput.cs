using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}