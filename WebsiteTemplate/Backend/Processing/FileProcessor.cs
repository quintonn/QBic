using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Controllers;

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

            if (!EventList.ContainsKey(eventId))
            {
                throw new Exception("No action has been found for event number: " + eventId);
            }

            var eventItem = EventList[eventId] as OpenFile;

            var fileInfo = eventItem.GetFileInfo(data);
            //return fileInfo;
            return new FileActionResult(fileInfo);
        }
    }
}