using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus
{
    public class ExecuteAction : Event
    {
        public EventNumber EventNumber { get; private set; }

        public ExecuteAction()
        {

        }

        public ExecuteAction(EventNumber eventNumber)
        {
            EventNumber = eventNumber;
        }

        public override EventNumber Id
        {
            get
            {
                return EventNumber.ExecuteAction;
            }
        }

        public override string Name
        {
            get
            {
                return "Execute Action";
            }
        }

        public override string Description
        {
            get
            {
                return "Execute Action";
            }
        }

        public override IList<SiteSpecific.UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.ExecuteAction;
            }
        }
    }
}