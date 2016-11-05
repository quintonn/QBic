using DocumentGenerator.DocumentTypes;
using DocumentGenerator.Settings;
using DocumentGenerator.Styles;
using MigraDoc.DocumentObjectModel;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.AuditReport
{
    public class CreateAuditReport : OpenFile
    {
        public override string Description
        {
            get
            {
                return "Create Audit Report";
            }
        }

        private StyleSetup StyleSetup { get; set; }
        private DataService DataService { get; set; }

        public CreateAuditReport(StyleSetup styleSetup, DataService dataService)
        {
            StyleSetup = styleSetup;
            DataService = dataService;
        }

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var json = JsonHelper.Parse(data);

            var fromDate = json.GetValue<DateTime>("FromDate");
            var toDate = json.GetValue<DateTime>("ToDate");
            var userIdString = json.GetValue("UserIds");
            var userIds = JArray.Parse(userIdString).Select(u => u.ToString()).ToArray();

            var document = new BasicTableLayoutDocument(StyleSetup, new DocumentSettings(DocumentType.Pdf, Orientation.Landscape));
            document.SetDocumentTitle("Audit Report: from " + fromDate.ToShortDateString() + " to " + toDate.ToShortDateString()); //TODO: need a subheading in report

            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync();

            var footer = "Printed by " + user.UserName + " on " + System.DateTime.Now.ToString("yyyy-MM-dd");
            document.SetDocumentFooter(footer);
            document.SetSideMargin(Unit.FromCentimeter(1));

            document.AddRowHeading("Date Time\t\tAction\tEntity");
            document.AddRowHeading("Tmp", "");
            //document.AddRowHeading("Action");
            //document.AddRowHeading("Entity");
            //document.AddRowHeading("Original Value");
            //document.AddRowHeading("New Value");

            using (var session = DataService.OpenSession())
            {
                var auditEvents = session.CreateCriteria<AuditEvent>()
                                         .CreateAlias("User", "user")
                                         .Add(Restrictions.Gt("AuditEventDateTimeUTC", fromDate.AddDays(-1)))
                                         .Add(Restrictions.Lt("AuditEventDateTimeUTC", toDate.AddDays(1)))
                                         .Add(Restrictions.In("user.Id", userIds))
                                         .AddOrder(Order.Asc("AuditEventDateTimeUTC"))
                                         .AddOrder(Order.Asc("EntityName"))
                                         .List<AuditEvent>()
                                         .ToList();
                foreach (var audit in auditEvents)
                {
                    var origObj = audit.OriginalObject;
                    var newObj = audit.NewObject.Replace(",\"", ", \"");
                    document.AddRowUsingParams(audit.AuditEventDateTimeUTC.ToString() + "\t" + audit.AuditAction.ToString() + "\t\t" + audit.EntityName +"\n"+origObj, "\n"+newObj);
                    //document.AddRowUsingParams(origObj, newObj);
                }
            }

            var result = new FileInfo();
            using (var stream = document.GenerateDocument(false))
            {
                result.Data = stream.ToArray();
                result.FileName = document.DocumentTitle;
                result.MimeType = "application/pdf";
            }

            return result;
        }

        public override EventNumber GetId()
        {
            return EventNumber.CreateAuditReport;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
    }
}