using QBic.Core.Utilities;
using System;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace WebsiteTemplate.Backend.Processing
{
    public class FileProcessor : CoreProcessor<FileActionResult>
    {
        public FileProcessor(IServiceProvider container)
            : base(container)
        {

        }

        public async override Task<FileActionResult> ProcessEvent(int eventId)
        {
            var data = await GetRequestData();
            if (String.IsNullOrWhiteSpace(data))
            {
                data = Container.GetService<IHttpContextAccessor>().HttpContext.Request.Query["requestData"];
            }

            if (!String.IsNullOrWhiteSpace(data))
            {
                data = QBicUtils.Base64Decode(data);
            }

            var eventItem = Container.GetService<EventService>().GetEventItem(eventId) as OpenFile;
            if (eventItem == null)
            {
                throw new Exception("No OpenFile action has been found for event number: " + eventId);
            }

            //var eventItem = EventList[eventId] as OpenFile;
            //var eventItemType = EventList[eventId];
            //var eventItem = Container.Resolve(eventItemType) as OpenFile;


            //var __ignore__ = eventItem.FileName; /* Leave this here -> this initializes the filename */
            var fileInfo = await eventItem.GetFileInfo(data);
            //return fileInfo;
            return new FileActionResult(fileInfo);
        }
    }
}