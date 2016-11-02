using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ViewBackgroundDetail : DoSomething
    {
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
            var id = GetValue<int>("Id");

            string message;
            if (type == "status")
            {
                message = BackgroundService.StatusInfo[id];
            }
            else if (type == "errors")
            {
                message = BackgroundService.Errors[id];
            }
            else
            {
                message = "UNKOWN TYPE";
            }

            var result = new List<IEvent>()
            {
                new ShowMessage(message)
            };

            return result;
        }
    }
}