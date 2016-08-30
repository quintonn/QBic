using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class InputEventProcessor : EventProcessor<IList<IEvent>>
    {
        public InputEventProcessor(IUnityContainer container)
            :base(container)
        {

        }

        public async override Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            var data = GetRequestData();
            var json = JsonHelper.Parse(data);

            var formData = json.GetValue("Data");
            var actionId = json.GetValue<int>("ActionId");

            var id = eventId;
            var eventItem = EventList[id] as GetInput;

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

            await eventItem.Initialize(formData);
            foreach (var inputField in eventItem.InputFields)
            {
                var value = inputField.GetValue(jsonData.GetValue<JToken>(inputField.InputName));
                processedFormData.Add(inputField.InputName, value);
            }
            using (var session = DataService.OpenSession())
            {
                eventItem.InputData = processedFormData;
                eventItem.DataService = DataService;
                result = await eventItem.ProcessAction(actionId);

                await HandleProcessActionResult(result, eventItem);
                session.Flush();
            }
            foreach (var item in result)
            {
                item.Parameters = callParameters;
            }

            return result;
        }
    }
}