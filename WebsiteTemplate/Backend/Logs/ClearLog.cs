using NHibernate.Util;
using QBic.Core.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Logs
{
    public class ClearLog : DoSomething
    {
        public ClearLog(DataService dataService) : base(dataService)
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
                return "Clear System Log";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.ClearSystemLog;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var logsPath = QBicUtils.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Logs";
            var directoryInfo = new DirectoryInfo(logsPath);
            directoryInfo.GetFiles().OrderBy(x => x.LastAccessTime).ToList().ForEach(f =>
            {
                f.Delete();
            });
           

            return new List<IEvent>()
            {
                new ShowMessage("Log cleared"),
                new ExecuteAction(EventNumber.ViewSystemLog)
            };
        }
    }
}