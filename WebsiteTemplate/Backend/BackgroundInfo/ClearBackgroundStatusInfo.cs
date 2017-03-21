using NHibernate.Criterion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

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
            using (var session = DataService.OpenSession())
            {
                var items = session.QueryOver<BackgroundInformation>().Where(Restrictions.Not(Restrictions.On<BackgroundInformation>(x => x.Information).IsLike("Error:", MatchMode.Start))).List().ToList();
                foreach (var item in items)
                {
                    session.Delete(item);
                }
                session.Flush();
            }
            return new List<IEvent>()
            {
                new ShowMessage("Info cleared"),
                new ExecuteAction(EventNumber.ViewBackgroundStatusInfo)
            };
        }
    }
}