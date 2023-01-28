using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.AuditReport
{
    public class AuditReportFilter : GetInput
    {
        public override string Description
        {
            get
            {
                return "Audit Report Filter";
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        private UserService UserService { get; set; }

        public AuditReportFilter(UserService userService, DataService dataService) : base(dataService)
        {
            UserService = userService;
        }

        public override IList<InputField> GetInputFields()
        {
            var list = new List<InputField>();

            list.Add(new DateInput("FromDate", "From Date", DateTime.Now.Date, null, true));
            list.Add(new DateInput("ToDate", "To Date", DateTime.Now.Date, null, true));

            var users = UserService.RetrieveUsers(0, int.MaxValue, String.Empty).ToDictionary(u => u.Id, u => (object)u.UserName);
            var listSelection = new ListSelectionInput("Users", "Users")
            {
                ListSource = users
            };
            list.Add(listSelection);

            return list;
        }
        public override EventNumber GetId()
        {
            return EventNumber.AuditReportFilter;
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            var fromDate = GetValue<DateTime>("FromDate");
            var toDate = GetValue<DateTime>("ToDate");
            var userIds = GetValue<List<string>>("Users");

            var json = new JsonHelper();
            json.Add("FromDate", fromDate);
            json.Add("ToDate", toDate);
            json.Add("UserIds", JsonHelper.SerializeObject(userIds));

            return new List<IEvent>()
            {
                new ExecuteAction(EventNumber.CreateAuditReport, json.ToString()),
                new CancelInputDialog(),
            };
        }
    }
}