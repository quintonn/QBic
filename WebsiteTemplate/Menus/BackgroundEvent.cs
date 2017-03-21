using Microsoft.Practices.Unity;
using System;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public abstract class BackgroundEvent : Event
    {
        private IUnityContainer Container { get; set; }

        //TODO: I don't like having to pass container around.
        //      But I can't pass BackgroundService because background service's constructor calls UserManager, which is not yet defined 
        //      when background events are obtained.
        //      A fix could be to ignore background event types when loading events.
        public BackgroundEvent(IUnityContainer container)
        {
            Container = container;
        }
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
            var backgroundService = Container.Resolve<BackgroundService>();
            backgroundService.AddBackgroundInformation(this.Description, info);
        }

        public void AddBackgroundError(string error)
        {
            var backgroundService = Container.Resolve<BackgroundService>();
            backgroundService.AddBackgroundError(Description, new Exception(error));
        }
    }
}