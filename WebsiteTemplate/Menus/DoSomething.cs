using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public abstract class DoSomething : Event
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.DoSomething;
            }
        }

        public abstract Task<IList<Event>> ProcessAction(Dictionary<string, object> inputData);
    }
}