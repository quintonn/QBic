using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ClearBackgroundStatusInfo : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Clear background status info";
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.ClearBackgroundStatusInfo;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            BackgroundService.StatusInfo.Clear();
            return new List<IEvent>()
            {
                new ShowMessage("Info cleared")
            };
        }
    }
}