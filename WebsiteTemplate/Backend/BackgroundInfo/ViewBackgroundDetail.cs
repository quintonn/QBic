using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ViewBackgroundDetail : DoSomething
    {
        public ViewBackgroundDetail(DataService dataService) : base(dataService)
        {
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override string Description
        {
            get
            {
                return "Show background detail";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewBackgroundDetail;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var parameters = InputData["ViewParameters"]?.ToString();
            var json = JsonHelper.Parse(parameters);
            var type = json.GetValue("type");
            var id = GetValue("Id");

            BackgroundInformation info;

            using (var session = DataService.OpenSession())
            {
                info = session.Get<BackgroundInformation>(id);

                //if (type == "status")
                //{
                //    //info = BackgroundService.StatusInfo.Where(s => s.Id == id).Single();
                //}
                //else if (type == "errors")
                //{
                //    //info = BackgroundService.Errors.Where(s => s.Id == id).Single();
                //}
                //else
                //{
                //    return new List<IEvent>()
                //{
                //    new ShowMessage("Unknown background info type: " + type)
                //};
                //}
            }

            var message = info.DateTimeUTC.ToShortDateString() + " " + info.DateTimeUTC.ToLongTimeString() + "<br/>" +
                          info.Task + "<br/>" +
                          info.Information;

            var result = new List<IEvent>()
            {
                new ShowMessage(message)
            };

            return result;
        }
    }
}