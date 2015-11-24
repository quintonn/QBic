using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Models
{
    public class Menu: BaseClass
    {
        public virtual string Name { get; set; }

        public virtual Menu ParentMenu { get; set; }

        public virtual EventNumber? Event { get; set; }
    }
}