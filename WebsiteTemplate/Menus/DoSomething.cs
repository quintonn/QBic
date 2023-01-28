using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Menus
{
    public abstract class DoSomething : InputProcessingEvent
    {
        protected DoSomething(DataService dataService) : base(dataService)
        {
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.DoSomething;
            }
        }

        public abstract Task<IList<IEvent>> ProcessAction();
    }
}