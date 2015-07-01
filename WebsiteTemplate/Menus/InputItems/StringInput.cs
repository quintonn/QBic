using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class StringInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Text;
            }
        }

        public StringInput(string name, string label)
            : base(name, label)
        {
        }
    }
}