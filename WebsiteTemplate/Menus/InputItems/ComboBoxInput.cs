﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class ComboBoxInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.ComboBox;
            }
        }

        public ComboBoxInput(string name, string label, object defaultValue = null)
            : base(name, label, defaultValue)
        {
            ListItems = new List<string>();
        }

        public List<string> ListItems { get; set; }
    }
}