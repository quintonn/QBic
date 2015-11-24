using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public class MenuItem
    {
        public string Label { get; set; }

        public EventNumber EventNumber { get; set; }

        public string ParametersToPass { get; set; }

        public MenuItem(string label, EventNumber eventNumber, string parametersToPass = null)
        {
            Label = label;
            EventNumber = eventNumber;
            ParametersToPass = parametersToPass;
        }
    }
}