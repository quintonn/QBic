using NHibernate.Criterion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.EventRoleAssociations
{
    public class ViewEventRoleAssociations : ShowView
    {
        public override string Description
        {
            get
            {
                return "Event Role Associations";
            }
        }

        public override IList<MenuItem> GetViewMenu()
        {
            var results = new List<MenuItem>();

            results.Add(new MenuItem("Back", EventNumber.ViewUserEvents));
            results.Add(new MenuItem("Add", EventNumber.AddEventRoleAssociation, EventId));

            return results;
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Event", "Event");
            columnConfig.AddStringColumn("Allowed User Role", "UserRole");

            columnConfig.AddButtonColumn("", "", ButtonTextSource.Fixed, "X",
                columnSetting: new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Show,
                    Conditions = new List<Condition>()
                   {
                       new Condition("CanDelete", Comparison.Equals, "true")
                   }
                },
                eventItem: new UserConfirmation("Delete Menu Item?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteEventRoleAssociation,
                }
            );
        }

        private string EventId { get; set; }

        public override IEnumerable GetData(string data)
        {
            using (var session = Store.OpenSession())
            {
                EventId = data;
                var eventNumber = Convert.ToInt32(data);
                var results = session.CreateCriteria<EventRoleAssociation>()
                                     .Add(Restrictions.Eq("Event", eventNumber))
                                     .List<EventRoleAssociation>()
                                     .Select(r => new
                                     {
                                         Event = r.Event.ToString(),
                                         UserRole = r.UserRole.Name,
                                         CanDelete = r.CanDelete,
                                         Id = r.Id
                                     })
                                     .OrderBy(e => e.Event)
                                     .ThenBy(e => e.UserRole)
                                     .ToList();
                return results;
            }
        }

        public override int GetId()
        {
            return EventNumber.ViewEventRoleAssociations;
        }
    }
}