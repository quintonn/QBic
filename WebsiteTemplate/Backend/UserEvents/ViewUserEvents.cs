using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Backend.UserEvents
{
    public class ViewUserEvents : ShowView
    {
        public override string Description
        {
            get
            {
                return "User Events";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Event Number", "EventNumber");
            columnConfig.AddStringColumn("Event Name", "EventName");

            columnConfig.AddButtonColumn("Allowed User Roles", 
                                         "", 
                                         ButtonTextSource.Fixed, 
                                         "...",
                                         null,
                                         new ExecuteAction(EventNumber.ViewEventRoleAssociations));
        }

        public override IEnumerable GetData(string data)
        {
            var items = typeof(EventNumber).GetFields().ToDictionary(e => e.GetValue(null).ToString(), e => e.Name);
            var results = items.Select(i => new
                                {
                                    EventNumber = i.Key,
                                    EventName = i.Value,
                                    Id = i.Key
                                })
                                .OrderBy(i => i.EventName)
                                .ToList();

            return results;
        }

        public override int GetId()
        {
            return EventNumber.ViewUserEvents;
        }

        public override IList<MenuItem> GetViewMenu()
        {
            return new List<MenuItem>();
        }
    }
}