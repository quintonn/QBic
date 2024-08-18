using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus.ViewItems.ViewDetail;

namespace WebsiteTemplate.Backend.Processing
{
    internal class ViewDetailProcessing : CoreProcessor<object>
    {
        public ViewDetailProcessing(IServiceProvider container) : base(container)
        {
        }

        public override async Task<object> ProcessEvent(int eventId)
        {
            var eventService = Container.GetService<EventService>();
            var eventItem = eventService.GetEventItem(eventId) as ViewDetailSection;

            if (eventItem == null)
            {
                throw new Exception("No view detail section configuration has been found for event number: " + eventId);
            }

            var originalData = await GetRequestData();

            var user = await GetLoggedInUser();
            var allowedEvents = GetAllowedEventsForUser(user.Id);

            var validDetailComponentIds = eventItem.GetDetailComponentIds(originalData).Where(x => allowedEvents.Contains(x)).ToList();

            var validDetailComponents = validDetailComponentIds.Select(x => eventService.GetEventItem(x) as ViewDetailComponent).ToList();

            var allowedMenus = GetAllowedEventsForUser(user.Id);

            var parentData = originalData; // Do i need to parse this ? maybe not

            var result = new
            {
                Components = validDetailComponents.Select(detail => new
                {
                    Columns = detail.DoConfigureColumns(allowedMenus),
                    Data = detail.GetData(new GetDataSettings(parentData, string.Empty, 1, 10, string.Empty, true)),
                    Id = detail.Id,
                    Title = detail.Title,
                    DataForGettingMenu = detail.DataForGettingMenu
                }).ToList(),
                Title = eventItem.Title,
            };

            return result;
        }
    }
}
