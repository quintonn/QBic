using System.Collections.Generic;

namespace WebsiteTemplate.Models
{
    public class Menu : BaseClass
    {
        public virtual string Name { get; set; }

        public virtual Menu ParentMenu { get; set; }

        public virtual List<Menu> SubMenus { get; set; }

        public virtual int? Event { get; set; }

        public virtual int Position { get; set; }

        public Menu()
        {
            Event = null;
            ParentMenu = null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}