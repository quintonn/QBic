using System.Collections;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

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
            columnConfig.AddStringColumn("Status", "Status", 3);
            columnConfig.AddHiddenColumn("Id");
            columnConfig.AddLinkColumn("", "Id", "Detail", EventNumber.ViewBackgroundDetail, parametersToPass: "Status"); parameters not passing
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            var cnt = 0;
            var result = BackgroundService.StatusInfo.Select(s => new
            {
                Status = s,
                Id = cnt++
            }).ToList();
            return result; ;
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