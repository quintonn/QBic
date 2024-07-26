using System.Collections;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems.ViewDetail
{
    /// <summary>
    /// This is for the individual views that are displayed withing a details section, when an item is selected in a view
    /// </summary>
    public abstract class ViewDetailComponent : ShowView
    {
        public sealed override bool AllowInMenu => false;

        public sealed override EventType ActionType => EventType.ViewDetailComponent;

        public abstract EventNumber EventId { get; }

        public sealed override EventNumber GetId() // this so it's easier to set id, why is this a method anyway. maybe fix?
        {
            return EventId;
        }

        public sealed override int GetDataCount(GetDataSettings settings)
        {
            return -1; // we'll just return all the data
        }

        public sealed override IEnumerable GetData(GetDataSettings settings)
        {
            return GetData(settings.ViewData);
        }

        public abstract IEnumerable GetData(string data);
    }
}
