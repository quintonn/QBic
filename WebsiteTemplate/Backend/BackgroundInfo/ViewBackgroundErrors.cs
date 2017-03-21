using NHibernate.Criterion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ViewBackgroundErrors : ShowView
    {
        private DataService DataService { get; set; }

        public ViewBackgroundErrors(DataService dataService)
        {
            DataService = dataService;
        }
        public override string Description
        {
            get
            {
                return "Background Errors";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Date", "Date");
            columnConfig.AddStringColumn("Task", "Task");
            columnConfig.AddStringColumn("Error", "Error", 3);
            columnConfig.AddHiddenColumn("Id");
            columnConfig.AddHiddenColumn("Id");
            var json = new JsonHelper();
            json.Add("type", "errors");
            columnConfig.AddLinkColumn("", "Id", "Detail", EventNumber.ViewBackgroundDetail, parametersToPass: json.ToString());
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            using (var session = DataService.OpenSession())
            {
                var items = session.QueryOver<BackgroundInformation>()
                                   .Where(Restrictions.On<BackgroundInformation>(x => x.Information).IsLike("Error:", MatchMode.Start))
                                   .OrderBy(i => i.DateTimeUTC).Desc
                                   .Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                                   .Take(settings.LinesPerPage)
                                   .List();
                var result = items.Select(s => new
                {
                    Date = s.DateTimeUTC.ToShortDateString() + " " + s.DateTimeUTC.ToLongTimeString(),
                    Task = s.Task,
                    Error = s.Information,
                    Id = s.Id
                }).ToList();
                return result;
            }

            //var info = BackgroundService.Errors.OrderByDescending(s => s.DateTimeUTC).Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
            //               .Take(settings.LinesPerPage)
            //               .ToList();
            //var result = info.Select(s => new
            //{
            //    Date = s.DateTimeUTC.ToShortDateString() + " " + s.DateTimeUTC.ToLongTimeString(),
            //    Task = s.Task,
            //    Error = s.Information,
            //    Id = s.Id
            //}).ToList();
            //return result;

            //var cnt = 0;
            //return BackgroundService.Errors
            //          .Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
            //          .Take(settings.LinesPerPage)
            //          .Select(e => new
            //            {
            //                Error = e,
            //                Id = cnt++
            //            });
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            //return BackgroundService.Errors.Count;
            using (var session = DataService.OpenSession())
            {
                var count = session.QueryOver<BackgroundInformation>().Where(Restrictions.On<BackgroundInformation>(x => x.Information).IsLike("Error:", MatchMode.Start)).RowCount();
                return count;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewBackgroundErrors;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var result = new List<MenuItem>();

            result.Add(new MenuItem("Clear", EventNumber.ClearBackgroundErrors));

            return result;
        }
    }
}