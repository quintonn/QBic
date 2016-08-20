using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class EventProcessor<T> : CoreProcessor<T>
    {
        public EventProcessor(IUnityContainer container)
            : base(container)
        {

        }

        protected async void HandleProcessActionResult(IList<IEvent> result, IEvent eventItem)
        {
            var jsonDataToUpdate = String.Empty;
            foreach (var item in result)
            {
                if (item is UpdateInputView)
                {
                    Dictionary<string, object> inputData;
                    if (eventItem is GetInput)
                    {
                        inputData = (eventItem as GetInput).InputData;
                    }
                    else if (eventItem is InputProcessingEvent)
                    {
                        inputData = (eventItem as InputProcessingEvent).InputData;
                    }
                    else
                    {
                        throw new Exception("Unknown eventItem for UpdateInputView result: " + eventItem.GetType().ToString());
                    }
                    if (inputData.ContainsKey("rowId"))
                    {
                        throw new Exception("Put rowid code back");
                        //(item as UpdateInputView).RowId = Convert.ToInt32(inputData["rowId"]);
                    }
                    if (String.IsNullOrWhiteSpace(jsonDataToUpdate))
                    {
                        if (inputData.ContainsKey("ViewData"))
                        {
                            inputData.Remove("ViewData");
                        }
                        jsonDataToUpdate = JsonHelper.FromObject(inputData).ToString();
                    }
                    (item as UpdateInputView).JsonDataToUpdate = jsonDataToUpdate;
                }
            }
        }
    }
}