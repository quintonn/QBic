using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public class MenuItem
    {
        public string Label { get; set; }

        public EventNumber EventNumber { get; set; }

        public MenuItem(string label, EventNumber eventNumber)
        {
            Label = label;
            EventNumber = eventNumber;
        }
    }
}