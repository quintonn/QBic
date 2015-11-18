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

        public BooleanInput(string name, string label, object defaultValue = null)
            : base(name, label, defaultValue)
        {
        }
    }
}