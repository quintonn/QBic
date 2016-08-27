using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using System.Threading.Tasks;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.Processing
{
    public class ActionExecutionProcessor : EventProcessor<IList<IEvent>>
    {
        public ActionExecutionProcessor(IUnityContainer container)
            : base(container)
        {

        }

        public async override Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            var originalData = GetRequestData();

            var json = JsonHelper.Parse(originalData);
            originalData = json.GetValue("Data");

            var data = originalData;
            if (!String.IsNullOrWhiteSpace(data))
            {
                try
                {
                    var tmp = JsonHelper.Parse(data);
                    if (tmp != null)
                    {
                        var subData = tmp.GetValue("data");

                        if (!String.IsNullOrWhiteSpace(subData))
                        {
                            data = subData;
                        }
                    }
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    //do nothing, data was not json data.
                    Console.WriteLine(ex.Message);
                    //data = "";
                }
            }

            var tmpJson = JsonHelper.Parse(originalData);
            var parameters = tmpJson.GetValue("parameters");

            var id = eventId;

            if (!EventList.ContainsKey(id))
            {
                throw new Exception("No action has been found for event number: " + id);
            }

            var result = new List<IEvent>();

            var eventItem = EventList[id];

            if (eventItem is ShowView)
            {
                var action = eventItem as ShowView;

                using (var session = DataService.OpenSession())
                {
                    data = originalData;
                    var parentData = data;

                    var dataJson = new JsonHelper();
                    if (!String.IsNullOrWhiteSpace(data) && !(eventItem is ViewForInput))
                    {
                        dataJson = JsonHelper.Parse(data);
                        parameters = dataJson.GetValue("parameters");
                    }
                    else
                    {
                        parentData = data;  // In case user modified parentData -> this smells??
                    }

                    var totalLines = action.GetDataCount(parentData, String.Empty);

                    var list = action.GetData(parentData, 1, 10, String.Empty);
                    action.ViewData = list;

                    totalLines = Math.Max(totalLines, list.Cast<object>().Count());

                    action.CurrentPage = 1;
                    action.LinesPerPage = 10;
                    action.TotalLines = totalLines;
                    action.Filter = String.Empty;

                    //clicking back in view many times breaks filter args- test with menus

                    result.Add(action);
                }
            }
            else if (eventItem is DoSomething)
            {
                var processedFormData = JsonHelper.DeserializeObject<Dictionary<string, object>>(data);
                if (processedFormData != null && processedFormData.ContainsKey("rowData"))
                {
                    throw new Exception("Put rowData code back");
                    //var rowData = processedFormData["rowData"].ToString();
                    //processedFormData = JsonHelper.DeserializeObject<Dictionary<string, object>>(rowData);
                }
                else if (processedFormData == null)
                {
                    processedFormData = new Dictionary<string, object>(); // Cannot/should not be null
                }

                if (!processedFormData.ContainsKey("ViewData") && !String.IsNullOrWhiteSpace(data))
                {
                    var viewData = String.Empty;
                    var tmpData = JsonHelper.Parse(data);
                    viewData = tmpData.GetValue("ViewData");
                    processedFormData.Add("ViewData", viewData);
                }

                (eventItem as DoSomething).InputData = processedFormData;
                (eventItem as DoSomething).DataService = DataService;
                var doResult = await (eventItem as DoSomething).ProcessAction();
                HandleProcessActionResult(doResult, eventItem);
                result.AddRange(doResult);
            }
            else if (eventItem is GetInput)
            {
                var inputResult = eventItem as GetInput;

                var initializeResult = await inputResult.Initialize(data);
                if (!initializeResult.Success)
                {
                    if (String.IsNullOrWhiteSpace(initializeResult.Error))
                    {
                        throw new Exception("There was an initialization error for ExecuteUIAction " + eventItem.GetId() + " but there are not error details.");
                    }
                    throw new Exception(initializeResult.Error);
                }

                result.Add(inputResult);
            }
            else if (eventItem is CancelInputDialog)
            {
                return new List<IEvent>();
            }
            else if (eventItem is OpenFile)
            {
                (eventItem as OpenFile).SetData(data);
                result.Add(eventItem);
            }
            else if (eventItem is UserConfirmation)
            {
                (eventItem as UserConfirmation).Data = originalData;
                result.Add(eventItem);
            }
            else
            {
                throw new Exception("ERROR: Unknown UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
            }

            foreach (var item in result)
            {
                item.Parameters = parameters;
            }

            return result;
        }
    }
}