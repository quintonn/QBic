using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public abstract class DoSomething : Event
    {
        public override UIActionType ActionType
        {
            get
            {
                return UIActionType.DoSomething;
            }
        }

        public abstract Task<IList<Event>> ProcessAction(string data);
    }
}