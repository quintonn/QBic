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
            //BackgroundService.Errors.Clear();
            using (var session = DataService.OpenStatelessSession())
            using (var transaction = session.BeginTransaction())
            {
                var items = session.QueryOver<BackgroundInformation>().Where(Restrictions.On<BackgroundInformation>(x => x.Information).IsLike("Error:", MatchMode.Start)).List().ToList();
                foreach (var item in items)
                {
                    session.Delete(item);
                }
                //session.Flush();
                transaction.Commit();
            }
            return new List<IEvent>()
            {
                new ShowMessage("Errors cleared"),
                new ExecuteAction(EventNumber.ViewBackgroundErrors)
            };
        }
    }
}