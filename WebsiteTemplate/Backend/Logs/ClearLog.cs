using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using static log4net.Appender.FileAppender;

namespace WebsiteTemplate.Backend.Logs
{
    public class ClearLog : DoSomething
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
                return "Clear System Log";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.ClearSystemLog;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var rootAppender = ((Hierarchy)LogManager.GetRepository())
                                         .Root.Appenders.OfType<FileAppender>()
                                         .FirstOrDefault();
            if (rootAppender != null)
            {
                File.WriteAllText(rootAppender.File, string.Empty);
            }

            return new List<IEvent>()
            {
                new ShowMessage("Log cleared"),
                new ExecuteAction(EventNumber.ViewSystemLog)
            };
        }
    }
}