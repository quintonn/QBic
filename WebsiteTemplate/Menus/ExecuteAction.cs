using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Menus
{
    public class ExecuteAction : Event
    {
        public EventNumber EventNumber { get; private set; }

        public string ParametersToPass { get; private set; }

        public ExecuteAction()
        {

        }

        public ExecuteAction(EventNumber eventNumber, string parametersToPass = null)
        {
            EventNumber = eventNumber;
            ParametersToPass = parametersToPass;
        }

        public override EventNumber GetId()
        {
            return EventNumber.ExecuteAction;
        }

        public override string Description
        {
            get
            {
                return "Execute Action";
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.ExecuteAction;
            }
        }
    }
}