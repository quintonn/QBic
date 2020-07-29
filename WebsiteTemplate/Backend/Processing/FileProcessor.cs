using QBic.Core.Utilities;
using System;
using System.Threading.Tasks;
using System.Web;
using Unity;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;

namespace WebsiteTemplate.Backend.Processing
{
    public class FileProcessor : CoreProcessor<FileActionResult>
    {
        public FileProcessor(IUnityContainer container)
            : base(container)
        {

        }

        public async override Task<FileActionResult> ProcessEvent(int eventId)
        {
            var data = GetRequestData();
            if (String.IsNullOrWhiteSpace(data))
            {
                data = HttpContext.Current.Request.Params["requestData"];
            }

            if (!String.IsNullOrWhiteSpace(data))
            {
                data = QBicUtils.Base64Decode(data);
            }

            var eventItem = Container.Resolve<EventService>().GetEventItem(eventId) as OpenFile;
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