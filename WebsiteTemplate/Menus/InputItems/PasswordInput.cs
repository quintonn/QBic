using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class PasswordInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Password;
            }
        }

        public PasswordInput(string name, string label)
            : base(name, label)
        {
        }
    }
}