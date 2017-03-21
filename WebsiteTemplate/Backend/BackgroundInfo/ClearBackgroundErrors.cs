using NHibernate.Criterion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ClearBackgroundErrors : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Clear background errors";
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
            return EventNumber.ClearBackgroundErrors;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            BackgroundService.Errors.Clear();
            return new List<IEvent>()
            {
                new ShowMessage("Errors cleared"),
                new ExecuteAction(EventNumber.ViewBackgroundErrors)
            };
        }
    }
}