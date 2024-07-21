using System.Collections;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ViewForInput : ShowView
    {
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
        public override EventType ActionType
        {
            get
            {
                return EventType.InputDataView;
            }
        }

        public override sealed IEnumerable GetData(GetDataSettings settings)
        {
            return null;
        }

        public override sealed int GetDataCount(GetDataSettings settings)
        {
            return 0;
        }
    }
}