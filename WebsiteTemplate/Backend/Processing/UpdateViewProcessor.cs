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

namespace WebsiteTemplate.Backend.Processing
{
    public class UpdateViewProcessor : CoreProcessor<Event>
    {
        public UpdateViewProcessor(IUnityContainer container)
            : base(container)
        {

        }

        public async override Task<Event> ProcessEvent(int eventId)
        {
            var user = GetLoggedInUser();
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

            if (!EventList.ContainsKey(id))
            {
                throw new Exception("No action has been found for event number: " + id);
            }

            var eventItem = EventList[id];

            if (!(eventItem is ShowView))
            {
                throw new Exception("ERROR: Invalid UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
            }
            var action = eventItem as ShowView;

            using (var session = DataService.OpenSession())
            {
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

                if (totalLines == -1 || !String.IsNullOrWhiteSpace(filter))
                {
                    totalLines = action.GetDataCount(parentData, filter);
                }

                var list = action.GetData(parentData, currentPage, linesPerPage, filter);
                action.ViewData = list;

                totalLines = Math.Max(totalLines, list.Cast<object>().Count());

                action.CurrentPage = currentPage;
                action.LinesPerPage = linesPerPage == int.MaxValue ? -2 : linesPerPage;
                action.TotalLines = totalLines;
                action.Filter = filter;
                action.Parameters = parameters;

                return action;
            }
        }
    }
}