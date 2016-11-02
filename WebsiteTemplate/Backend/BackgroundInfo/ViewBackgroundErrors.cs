using System.Collections;
using System.Linq;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.BackgroundInfo
{
    public class ViewBackgroundErrors : ShowView
    {
        public override string Description
        {
            get
            {
                return "View background errors";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Error", "Error", 3);
            columnConfig.AddHiddenColumn("Id");
            var json = new JsonHelper();
            json.Add("type", "errors");
            columnConfig.AddLinkColumn("", "Id", "Detail", EventNumber.ViewBackgroundDetail, parametersToPass: json.ToString());
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            var cnt = 0;
            return BackgroundService.Errors.Select(e => new
            {
                Error = e,
                Id = cnt++
            });
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            return BackgroundService.Errors.Count;
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