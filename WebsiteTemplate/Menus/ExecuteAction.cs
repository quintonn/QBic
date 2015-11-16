using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

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

        public override EventNumber GetId()
        {
            return EventNumber.ExecuteAction;
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