using QBic.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Backend.Logs
{
    public class ViewLogs : ShowView
    {
        public ViewLogs()
        {
            LogsCache = null;
        }
        private string[] LogsCache { get; set; }
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

        private string[] GetLogs(GetDataSettings settings)
        {
            if (LogsCache == null)
            {
                var logsPath = QBicUtils.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Logs";

                if (!Directory.Exists(logsPath))
                {
                    throw new Exception("Logs folder does not exist");
                }

                var directoryInfo = new DirectoryInfo(logsPath);
                var files = directoryInfo.GetFiles().OrderBy(p => p.LastWriteTime).TakeLast(5).ToList();

                LogsCache = files.SelectMany(f => SafelyReadAllLines(f.FullName, settings)).Reverse().ToArray();
            }
            return LogsCache;
        }

        public string[] SafelyReadAllLines(string path, GetDataSettings settings)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                var file = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (!string.IsNullOrWhiteSpace(settings.Filter))
                    {
                        if (!line.Contains(settings.Filter, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                    }

                    file.Add(line);
                }

                return file.ToArray();
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Data", "Data");
            columnConfig.AddLinkColumn("", "Data", "Details", EventNumber.ShowLogInfo);
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            var data = GetLogs(settings)
                           .Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                           .Take(settings.LinesPerPage);

            var result = data.Select(d => new
            {
                Data = d
            });

            return result;
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            return GetLogs(settings).Count();
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