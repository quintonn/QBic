using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudDelete : IEvent
    {
        int Id { get; set; }
        string ItemName { get; set; }
    }
}