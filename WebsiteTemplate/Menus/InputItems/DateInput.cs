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

        public DateInput(string name, string label, object defaultValue = null, string tabName = null)
            : base(name, label, defaultValue, tabName)
        {
        }
    }
}