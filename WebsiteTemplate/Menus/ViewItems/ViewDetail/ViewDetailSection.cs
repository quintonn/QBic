using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems.ViewDetail
{
    /// <summary>
    /// This is for the section to display when an item in a view is selected
    /// </summary>
    public abstract class ViewDetailSection : BaseViewEvent
    {
        public sealed override bool AllowInMenu => false;

        /// <summary>
        /// Returns the title that will be displayed when this view is rendered.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return Description;
            }
        }

        public abstract EventNumber EventId { get; }

        public sealed override EventNumber GetId() // this so it's easier to set id, why is this a method anyway. maybe fix?
        {
            return EventId;
        }

        public sealed override EventType ActionType => EventType.ViewDetailSection;

        /// <summary>
        /// The id's of the detail components to show for the selected row, defined in the request data parameter
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public abstract IList<EventNumber> GetDetailComponentIds(string requestData);
        // having this as an extra class allows different rows to have different detail components potentially
    }
}
