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