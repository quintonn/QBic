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

            results.Add(new MenuItem("Add", EventNumber.AddEventRoleAssociation));

            return results;
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Event", "Event");
            columnConfig.AddStringColumn("Allowed User Role", "UserRole");

            //columnConfig.AddLinkColumn("", "Edit", "Id", "Edit", EventNumber.EditEventRoleAssociation);

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
                    OnConfirmationUIAction = EventNumber.DeleteEventRoleAssociation
                }
            );
        }

        public override IEnumerable GetData(string data)
        {
            using (var session = Store.OpenSession())
            {
                var results = session.CreateCriteria<EventRoleAssociation>()
                                     .List<EventRoleAssociation>()
                                     .Select(r => new
                                     {
                                         Event = r.Event.ToString(),
                                         UserRole = r.UserRole.ToString(),
                                         CanDelete = r.CanDelete,
                                         Id = r.Id
                                     })
                                     .ToList();
                return results;
            }
        }

        public override Type GetDataType()
        {
            return typeof(EventRoleAssociation);
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewEventRoleAssociation;
        }
    }
}