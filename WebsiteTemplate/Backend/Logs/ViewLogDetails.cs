using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;

namespace WebsiteTemplate.Backend.Logs
{
    internal class ViewLogDetails : CoreAction
    {
        public ViewLogDetails(DataService dataService) : base(dataService)
        {
        }

        public override bool AllowInMenu => false;

        public override string Description => "View Log Details";

        public override EventNumber GetId()
        {
            return EventNumber.ShowLogInfo;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var info = GetValue("Data");
            return new List<IEvent>()
            {
                new ShowMessage(info),
            };
        }
    }
}
