using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Menus
{
    public abstract class DoSomething : InputProcessingEvent
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.DoSomething;
            }
        }

        public abstract Task<IList<Event>> ProcessAction();
    }
}