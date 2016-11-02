using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

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
            var x = InputData;
            var id = GetValue<int>("Id");
            var result = new List<IEvent>()
            {
                new ShowMessage(BackgroundService.StatusInfo[id])
            };

            return result;
        }
    }
}