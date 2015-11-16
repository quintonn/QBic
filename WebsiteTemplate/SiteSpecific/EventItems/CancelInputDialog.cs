using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class CancelInputDialog : Event
    {
        public override EventNumber GetId()
        {
            return EventNumber.CancelInputDialog;
        }

        public override string Description
        {
            get
            {
                return "Cancel Input Dialog";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.AnyOne
                };
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.CancelInputDialog;
            }
        }
    }
}