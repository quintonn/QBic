using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Menus.BaseItems
{
    public class CancelInputDialog : Event
    {
        public override int GetId()
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

        public override EventType ActionType
        {
            get
            {
                return EventType.CancelInputDialog;
            }
        }
    }
}