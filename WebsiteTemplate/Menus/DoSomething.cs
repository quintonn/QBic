﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
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

        public abstract Task<IList<IEvent>> ProcessAction();
    }
}