using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class PropertyChangeProcessor : CoreProcessor<IList<IEvent>>
    {
        public PropertyChangeProcessor(IUnityContainer container)
            :base(container)
        {

        }

        public async override Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            var data = GetRequestData();
            var json = JsonHelper.Parse(data);
            data = json.GetValue("Data");

            json = JsonHelper.Parse(data);

            //var eventId = json.GetValue<int>("EventId");
            var propertyName = json.GetValue("PropertyName");
            var propertyValue = json.GetValue<object>("PropertyValue");
            var eventItem = EventList[eventId] as GetInput;

            var result = await eventItem.OnPropertyChanged(propertyName, propertyValue);
            return result;
        }
    }
}