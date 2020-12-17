using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public class LogoutEvent : Event
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.Logout;
            }
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override bool RequiresAuthorization
        {
            get
            {
                return false;
            }
        }

        public override string Description
        {
            get
            {
                return "Log the current user out of the application";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.Logout;
        }
    }
}