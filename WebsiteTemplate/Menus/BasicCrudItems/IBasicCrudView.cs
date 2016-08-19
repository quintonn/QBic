using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudView : IEvent
    {
        EventNumber Id { get; set; }
        string ItemName { get; set; }
        Dictionary<string, string> ColumnsToShowInView { get; set; }
        DataStore Store { get; set; }
    }
}