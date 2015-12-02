using WebsiteTemplate.Mappings;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Models
{
    public class EventRoleAssociation : DynamicClass
    {
        public virtual int Event { get; set; }

        public virtual UserRole UserRole { get; set; }
    }
}