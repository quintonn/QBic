using System.Collections;
using System.Linq;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

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
            columnConfig.AddStringColumn("Error", "Error");
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            return BackgroundService.Errors.Select(e => new
            {
                Error = e
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