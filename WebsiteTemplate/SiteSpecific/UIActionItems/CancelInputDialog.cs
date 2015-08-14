using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class CancelInputDialog : Event
    {
        public override EventNumber Id
        {
            get
            {
                return EventNumber.CancelInputDialog;
            }
        }

        public override string Name
        {
            get
            {
                return "Cancel";
            }
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