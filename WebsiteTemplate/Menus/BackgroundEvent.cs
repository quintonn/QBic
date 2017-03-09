using System;
using WebsiteTemplate.Backend.Services;
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

        public void AddBackgroundInfo(string info)
        {
            BackgroundService.AddToStatusInfo(this.Description, info);
        }

        public void AddBackgroundError(string error)
        {
            BackgroundService.AddError(Description, new Exception(error));
        }
    }
}