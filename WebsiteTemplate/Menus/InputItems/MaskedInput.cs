using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class MaskedInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.MaskedInput;
            }
        }

        public string InputMask { get; set; }

        public MaskedInput(string name, string label, string inputMask, object defaultValue = null)
            : base(name, label, defaultValue)
        {
            InputMask = inputMask;
        }
    }
}