using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class ActionExecutionProcessor : EventProcessor<IList<IEvent>>
    {
        public ActionExecutionProcessor(IServiceProvider container)
            : base(container)
        {

        }

        public async override Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            var originalData = await GetRequestData();

            var json = JsonHelper.Parse(originalData);
            originalData = json.GetValue("Data");

            var data = originalData;
            if (!string.IsNullOrWhiteSpace(data))
            {
                try
                {
                    var tmp = JsonHelper.Parse(data);
                    if (tmp != null)
                    {
                        var subData = tmp.GetValue("data");

                        if (!string.IsNullOrWhiteSpace(subData))
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
            var eventItem = Container.GetService<EventService>().GetEventItem(eventId);
            if (eventItem == null)
            {
                throw new Exception("No action has been found for event number: " + id);
            }

            var result = new List<IEvent>();
            //var eventItem = Container.GetService<EventService>().GetEventItem(id);
            //var eventItemType = EventList[id];
            //var eventItem = Container.Resolve(eventItemType) as IEvent;

            if (eventItem is ShowView)
            {
                var action = eventItem as ShowView;

                var user = await GetLoggedInUser();
                var allowedMenus = GetAllowedEventsForUser(user.Id);
                action.Columns = action.DoConfigureColumns(allowedMenus);

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

                if (action.DetailSectionId != null)
                {
                    if (!allowedMenus.Contains(action.DetailSectionId))
                    {
                        action.DetailSectionId = null;
                    }
                }

                var viewDataSettings = new GetDataSettings(parentData, string.Empty, 1, 10, string.Empty, true);

                var totalLines = action.GetDataCount(viewDataSettings);

                var list = action.GetData(viewDataSettings); //TODO: Instead of calling this here, i should re-use what's in UpdateViewProcessor
                action.ViewData = list;

                totalLines = Math.Max(totalLines, list.Cast<object>().Count());

                action.CurrentPage = 1;
                action.LinesPerPage = 10;
                action.TotalLines = totalLines;
                action.Filter = string.Empty;

                //clicking back in view many times breaks filter args- test with menus

                result.Add(action);
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
                    if (!String.IsNullOrWhiteSpace(tmpData?.ToString()))
                    {
                        viewData = tmpData.GetValue("ViewData");
                        processedFormData.Add("ViewData", viewData);
                    }
                    else
                    {
                        processedFormData.Add("ViewData", data);
                    }
                }

                if (!String.IsNullOrWhiteSpace(parameters))
                {
                    processedFormData.Add("ViewParameters", parameters);
                }

                (eventItem as DoSomething).InputData = processedFormData;
                var doResult = await (eventItem as DoSomething).ProcessAction();
                await HandleProcessActionResult(doResult, eventItem);
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

                var inputFields = inputResult.GetInputFields();
                for (var i = 0; i < inputFields.Count; i++)
                {
                    var input = inputFields[i];
                    if (input.InputType == InputType.View)
                    {
                        var user = await GetLoggedInUser();
                        var allowedMenus = GetAllowedEventsForUser(user?.Id);

                        var columns = (input as IViewInput).ViewForInput.DoConfigureColumns(allowedMenus);
                        (input as IViewInput).ViewForInput.Columns = columns;
                    }
                }
                inputFields.Add(new HiddenInput("__init_data__", data)); // I think the init data is just there for the CoreModify class and only to actually get the Id field
                                                                         // Can be simplified
                inputResult.InputFields = inputFields;
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
            else if (eventItem is LogoutEvent)
            {
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