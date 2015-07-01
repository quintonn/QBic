using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class InputField
    {
        public abstract InputType InputType { get; }

        public string InputName { get; set; }

        public string InputLabel { get; set; }

        public InputField(string name, string label)
        {
            InputName = name;
            InputLabel = label;
        }
    }
}