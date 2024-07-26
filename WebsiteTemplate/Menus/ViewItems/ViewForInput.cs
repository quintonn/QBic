using System.Collections;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ViewForInput : ShowView
    {
        public sealed override bool AllowInMenu => false;
        public sealed override EventType ActionType => EventType.InputDataView;

        // seal the get data methods because the data must be passed into the constructor now, easiest way for front-end i think
        // the data is passed in as default value in the ViewInput input type, could be better?
        // maybe just a getData method ? it seems to work, so maybe not
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