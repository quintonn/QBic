using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudModify : IEvent
    {
        int Id { get; set; }
        string ItemName { get; set; }
        Dictionary<string, string> InputProperties { get; set; }

        string UniquePropertyName { get; set; }
    }
}