using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class InputEventProcessor : EventProcessor<IList<IEvent>>
    {
        public InputEventProcessor(IServiceProvider container)
            : base(container)
        {

        }

        public async override Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            var data = await GetRequestData();
            var json = JsonHelper.Parse(data);

            var formData = json.GetValue("Data");
            var actionId = json.GetValue<int>("ActionId");

            var id = eventId;
            var eventItem = Container.GetService<EventService>().GetEventItem(eventId) as GetInput;
            //var eventItemType = EventList[id];
            //var eventItem = Container.Resolve(eventItemType) as GetInput;

            var inputButtons = eventItem.InputButtons;
            if (inputButtons.Where(i => i.ActionNumber == actionId).Count() == 0)
            {
                return new List<IEvent>()
                {
                    new ShowMessage("No button with action number " + actionId + " exists for " + eventItem.Description),
                };
            };

            var callParameters = String.Empty;

            var jsonData = JsonHelper.Parse(formData);
            callParameters = jsonData.GetValue("parameters");

            IList<IEvent> result;

            var processedFormData = new Dictionary<string, object>();

            //await eventItem.Initialize(formData); //TODO: Do i need to call this here???? WHY!! If it's a must, maybe i should pass a flag that it's not the real initialize.
            //TODO: One instance where this causes errors is if an input screen is open and I rebuild the back-end, then clicking cancel results in error.

            //TODO: getInputFields doesn't return consistent data
            //      it needs to be sent the same info it was sent when the screen was created so it knows how to get inputs
            //      means i also need to call initialize.
            //
            
            var initData = jsonData.GetValue("__init_data__")?.ToString();
            await eventItem.Initialize(initData);
            foreach (var inputField in eventItem.GetInputFields())
            {
                var value = inputField.GetValue(jsonData.GetValue<JToken>(inputField.InputName));
                processedFormData.Add(inputField.InputName, value);
            }

            eventItem.InputData = processedFormData;
            eventItem.DataService = DataService;
            result = await eventItem.ProcessAction(actionId);

            await HandleProcessActionResult(result, eventItem);
            foreach (var item in result)
            {
                item.Parameters = callParameters;
            }

            return result;
        }
    }
}