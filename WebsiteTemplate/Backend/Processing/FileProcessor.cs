using Unity;
using System;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Utilities;
using Benoni.Core.Utilities;

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
                data = BenoniUtils.Base64Decode(data);
            }

            if (!EventList.ContainsKey(eventId))
            {
                throw new Exception("No action has been found for event number: " + eventId);
            }

            var eventItem = EventList[eventId] as OpenFile;
            //var __ignore__ = eventItem.FileName; /* Leave this here -> this initializes the filename */
            var fileInfo = await eventItem.GetFileInfo(data);
            //return fileInfo;
            return new FileActionResult(fileInfo);
        }
    }
}