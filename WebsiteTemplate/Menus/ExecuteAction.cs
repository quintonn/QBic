using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Menus
{
    public class ExecuteAction : Event
    {
        public int EventNumber { get; private set; }

        public string ParametersToPass { get; private set; }

        public ExecuteAction()
        {

        }

        public ExecuteAction(int eventNumber, string parametersToPass = null)
        {
            EventNumber = eventNumber;
            ParametersToPass = parametersToPass;
        }

        public override EventNumber GetId()
        {
            return Menus.BaseItems.EventNumber.ExecuteAction;
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