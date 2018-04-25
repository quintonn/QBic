using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus;

namespace WebsiteTemplate.Backend.Logs
{
    public class ViewLogs : ShowView
    {
        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override string Description
        {
            get
            {
                return "View System Log";
            }
        }

        private List<string> GetLogs()
        {
            var rootAppender = ((Hierarchy)LogManager.GetRepository())
                                         .Root.Appenders.OfType<FileAppender>()
                                         .FirstOrDefault();
            if (rootAppender != null)
            {
                return File.ReadAllLines(rootAppender.File).Reverse().ToList();

                //change log level
                //((Hierarchy)LogManager.GetRepository()).Root.Level

                // Read without locking:
                //using (var fileStream = new FileStream(rootAppender.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                //{
                //    using (var streamReader = new StreamReader(fileStream))
                //    {   
                //        var data = streamReader.ReadToEnd();
                //        var result = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();
                //        return result;
                //    }
                //}
            }

            return new List<string>();
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Data", "Data");
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            var data = GetLogs()
                           .Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                           .Take(settings.LinesPerPage);

            //var result = new List<object>();
            var result = data.Select(d => new
            {
                Data = d
            });

            return result;
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            return GetLogs().Count();
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewSystemLog;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>()
            {
                new MenuItem("Clear", EventNumber.ClearSystemLog)
            };
        }
    }
}