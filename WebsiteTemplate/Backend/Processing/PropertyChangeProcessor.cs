using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class PropertyChangeProcessor : CoreProcessor<IList<IEvent>>
    {
        public PropertyChangeProcessor(IServiceProvider container, ILogger<PropertyChangeProcessor> logger)
            :base(container, logger)
        {

        }

        public async override Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            var data = await GetRequestData();
            var json = JsonHelper.Parse(data);
            data = json.GetValue("Data");

            json = JsonHelper.Parse(data);

            //var eventId = json.GetValue<int>("EventId");
            var propertyName = json.GetValue("PropertyName");
            var propertyValue = json.GetValue<object>("PropertyValue");
            //var eventItem = EventList[eventId] as GetInput;
            //var eventItemType = EventList[eventId];
            //var eventItem = Container.Resolve(eventItemType) as GetInput;
            var eventItem = Container.GetService<EventService>().GetEventItem(eventId) as GetInput;

            var result = await eventItem.OnPropertyChanged(propertyName, propertyValue);
            return result;
        }
    }
}