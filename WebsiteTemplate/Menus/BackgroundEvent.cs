using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public abstract class BackgroundEvent : Event
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.BackgroundEvent;
            }
        }

        public override bool RequiresAuthorization
        {
            get
            {
                return false;
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public abstract void DoWork();

        public abstract DateTime CalculateNextRunTime(DateTime? lastRunTime);

        public abstract bool RunImmediatelyFirstTime { get; }

    }
}