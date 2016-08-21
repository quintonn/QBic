using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using System.Threading.Tasks;
using WebsiteTemplate.Utilities;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Processing
{
    public class ViewMenuProcessor : CoreProcessor<IList<MenuItem>>
    {
        public ViewMenuProcessor(IUnityContainer container)
            : base(container)
        {

        }

        public async override Task<IList<MenuItem>> ProcessEvent(int eventId)
        {
            var postData = GetRequestData();
            var data = String.Empty;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                var json = JsonHelper.Parse(postData);
                data = json.GetValue("data");
            }

            if (!EventList.ContainsKey(eventId))
            {
                throw new Exception("No action has been found for event number: " + eventId);
            }

            var eventItem = EventList[eventId] as ShowView;

            var dataForMenu = new Dictionary<string, string>();
            if (!String.IsNullOrWhiteSpace(data))
            {
                dataForMenu = JsonHelper.DeserializeObject<Dictionary<string, string>>(data);
            }
            var viewMenu = eventItem.GetViewMenu(dataForMenu);

            var user = await GetLoggedInUser();
            List<MenuItem> allowedMenuItems;
            using (var session = Store.OpenSession())
            {

                var allowedEvents = GetAllowedEventsForUser(session, user.Id);
                allowedMenuItems = viewMenu.Where(m => allowedEvents.Contains(m.EventNumber)).ToList();
            }

            return allowedMenuItems;
        }

    }
}