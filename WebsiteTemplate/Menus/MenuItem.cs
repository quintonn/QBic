using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public class MenuItem
    {
        public string Label { get; set; }

        public int EventNumber { get; set; }

        public string ParametersToPass { get; set; }

        /// <summary>
        /// This indicates whether or not to return the data present in the view when this item is a view menu item.
        /// Default is false.
        /// </summary>
        public bool IncludeDataInView { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label">Menu label that user will see.</param>
        /// <param name="eventNumber">Event to execute when menu is selected.</param>
        /// <param name="parametersToPass">Any additional parameters to pass to event when menu is selected.</param>
        /// <param name="includeDataInView">This indicates whether or not to return the data present in the view when this item is a view menu item.</param>
        public MenuItem(string label, EventNumber eventNumber, string parametersToPass = null, bool includeDataInView = false)
        {
            Label = label;
            EventNumber = eventNumber;
            ParametersToPass = parametersToPass;
            IncludeDataInView = includeDataInView;
        }

        //TODO: THis will be more consistent if it's working

        //public MenuItem(string label, int eventNumber, JsonHelper parametersToPass)
        //{
        //    Create(label, eventNumber, parametersToPass);
        //}

        //private void Create(string label, int eventNumber, JsonHelper parametersToPass)
        //{
        //    Label = label;
        //    EventNumber = eventNumber;
        //    ParametersToPass = parametersToPass?.ToString();
        //}
    }
}