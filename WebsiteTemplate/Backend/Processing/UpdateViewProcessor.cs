using System;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace WebsiteTemplate.Backend.Processing
{
    public class UpdateViewProcessor : CoreProcessor<Event>
    {
        public UpdateViewProcessor(IServiceProvider container)
            : base(container)
        {

        }

        public async override Task<Event> ProcessEvent(int eventId)
        {
            var user = await GetLoggedInUser();
            var originalData = await GetRequestData();

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
                        if (subData != null)
                        {
                            data = subData.ToString();
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

            var id = eventId;

            var eventItem = Container.GetService<EventService>().GetEventItem(eventId);

            //if (!EventList.ContainsKey(id))
            if (eventItem == null)
            {
                throw new Exception("No action has been found for event number: " + id);
            }

            //var eventItemType = EventList[id];
            //var eventItem = Container.Resolve(eventItemType) as IEvent;

            if (!(eventItem is ShowView))
            {
                throw new Exception("ERROR: Invalid UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
            }
            var action = eventItem as ShowView;

            data = originalData;
            var parentData = data;

            var currentPage = 1;
            var linesPerPage = 10;
            var totalLines = -1;
            var filter = String.Empty;
            var parameters = String.Empty;

            var dataJson = new JsonHelper();
            if (!String.IsNullOrWhiteSpace(data))
            {
                try
                {
                    dataJson = JsonHelper.Parse(data);

                    filter = dataJson.GetValue("filter").Trim();
                    parameters = dataJson.GetValue("parameters");

                    var viewSettings = dataJson.GetValue<JsonHelper>("viewSettings");
                    if (viewSettings != null)
                    {
                        currentPage = viewSettings.GetValue<int>("currentPage");
                        linesPerPage = viewSettings.GetValue<int>("linesPerPage");
                        if (linesPerPage == -2)
                        {
                            currentPage = 1; //just in case it's not
                            linesPerPage = int.MaxValue;
                        }
                        totalLines = viewSettings.GetValue<int>("totalLines");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                parentData = data;  // In case user modified parentData -> this smells??
            }

            var viewDataSettings = new GetDataSettings(parentData, filter, currentPage, linesPerPage);

            if (totalLines == -1 || !String.IsNullOrWhiteSpace(filter))
            {
                totalLines = action.GetDataCount(viewDataSettings);
            }

            var allowedMenus = GetAllowedEventsForUser(user.Id);
            action.Columns = action.DoConfigureColumns(allowedMenus);

            var list = action.GetData(viewDataSettings).Cast<object>().ToList();
            action.ViewData = list;

            totalLines = Math.Max(totalLines, list.Count);

            action.CurrentPage = currentPage;
            action.LinesPerPage = linesPerPage == int.MaxValue ? -2 : linesPerPage;
            action.TotalLines = totalLines;
            action.Filter = filter;
            action.Parameters = parameters;

            return action;
        }
    }
}