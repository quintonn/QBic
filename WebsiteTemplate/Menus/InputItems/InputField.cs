﻿using System.Collections.Generic;
using WebsiteTemplate.Menus.ViewItems;
namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class InputField
    {
        public abstract InputType InputType { get; }

        public string InputName { get; set; }

        public string InputLabel { get; set; }

        public object DefaultValue { get; set; }

        public List<Condition> VisibilityConditions { get; set; }

        public InputField(string name, string label, object defaultValue)
        {
            InputName = name;
            InputLabel = label;
            DefaultValue = defaultValue;

            VisibilityConditions = new List<Condition>();
        }
    }
}