using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using Microsoft.Extensions.DependencyInjection;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class ViewMenuProcessor : CoreProcessor<IList<MenuItem>>
    {
        public ViewMenuProcessor(IServiceProvider container)
            : base(container)
        {

        }

        public async override Task<IList<MenuItem>> ProcessEvent(int eventId)
        {
            var postData = await GetRequestData();
            var data = String.Empty;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                var json = JsonHelper.Parse(postData);
                data = json.GetValue("data");
            }

            var eventItem = Container.GetService<EventService>().GetEventItem(eventId) as ShowView;
            if (eventItem == null)
            {
                throw new Exception("No action has been found for event number: " + eventId);
            }

            //var eventItem = EventList[eventId] as ShowView;
            //var eventItemType = EventList[eventId];
            //var eventItem = Container.Resolve(eventItemType) as ShowView;
            //var eventItem = Container.GetService<EventService>().GetEventItem(eventId) as ShowView;

            var dataForMenu = new Dictionary<string, string>();
            if (!String.IsNullOrWhiteSpace(data))
            {
                dataForMenu = JsonHelper.DeserializeObject<Dictionary<string, string>>(data);
            }
            var viewMenu = eventItem.GetViewMenu(dataForMenu);

            var user = await GetLoggedInUser();
            List<MenuItem> allowedMenuItems;
            using (var session = DataService.OpenSession())
            {

                var allowedEvents = GetAllowedEventsForUser(user.Id);
                allowedMenuItems = viewMenu.Where(m => allowedEvents.Contains(m.EventNumber)).ToList();
            }

            return allowedMenuItems;
        }

    }
}