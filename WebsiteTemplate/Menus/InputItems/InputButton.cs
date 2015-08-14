using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class InputButton
    {
        public string Label { get; set; }

        public int ActionNumber { get; set; }

        public InputButton(string label, int actionNumber)
        {
            Label = label;
            ActionNumber = actionNumber;
        }
    }
}