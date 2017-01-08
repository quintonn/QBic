using System.Collections;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ViewBackgroundStatusInfo : ShowView
    {
        public override string Description
        {
            get
            {
                return "Background status info";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Date", "Date");
            columnConfig.AddStringColumn("Task", "Task");
            columnConfig.AddStringColumn("Status", "Status", 3);
            columnConfig.AddHiddenColumn("Id");
            var json = new JsonHelper();
            json.Add("type", "status");
            columnConfig.AddLinkColumn("", "Id", "Detail", EventNumber.ViewBackgroundDetail, parametersToPass: json.ToString()); //parameters not passing
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            var info = BackgroundService.StatusInfo.OrderByDescending(s => s.DateTimeUTC).Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                           .Take(settings.LinesPerPage)
                           .ToList();
            var result = info.Select(s => new
            {
                Date = s.DateTimeUTC.ToShortDateString() + " " + s.DateTimeUTC.ToLongTimeString(),
                Task = s.Task,
                Status = s.Information,
                Id = s.Id
            }).ToList();
            return result;
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            return BackgroundService.StatusInfo.Count;
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewBackgroundStatusInfo;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }
    }
}