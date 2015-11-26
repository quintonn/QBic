using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.EventRoleAssociations
{
    public class DeleteEventRoleAssociation : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Delete an event role association";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteEventRoleAssociation;
        }

        public override async Task<IList<Event>> ProcessAction(string data)
        {
            var json = JObject.Parse(data);

            var id = json.GetValue("Id").ToString();

            EventNumber eventNumber;
            using (var session = Store.OpenSession())
            {
                var eventRoleAssociation = session.Get<EventRoleAssociation>(id);
                eventNumber = eventRoleAssociation.Event;
                session.Delete(eventRoleAssociation);
                session.Flush();
            }

            return new List<Event>()
            {
                new ShowMessage("Event role association deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewEventRoleAssociations, ((int)eventNumber).ToString())
            };
        }
    }
}