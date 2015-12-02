using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public class MenuItem
    {
        public string Label { get; set; }

        public int EventNumber { get; set; }

        public string ParametersToPass { get; set; }

        public MenuItem(string label, int eventNumber, string parametersToPass = null)
        {
            Label = label;
            EventNumber = eventNumber;
            ParametersToPass = parametersToPass;
        }
    }
}