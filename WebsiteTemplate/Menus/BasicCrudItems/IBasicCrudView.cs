using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudView : IEvent
    {
        EventNumber Id { get; set; }
        string ItemName { get; set; }
        Dictionary<string, string> ColumnsToShowInView { get; set; }

        IList<ViewColumn> AdditionalColumns { get; set; }
    }
}