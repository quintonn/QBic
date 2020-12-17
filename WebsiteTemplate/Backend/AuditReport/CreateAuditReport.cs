using DocumentGenerator.DocumentTypes;
using DocumentGenerator.Settings;
using DocumentGenerator.Styles;
using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MigraDoc.DocumentObjectModel;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using Qactus.Authorization.Core;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
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
        private DataStore DataStore { get; set; }
        private UserManager<IUser> UserContext { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        public CreateAuditReport(StyleSetup styleSetup, DataStore dataStore, UserManager<IUser> userContext, IHttpContextAccessor httpContextAccessor)
        {
            StyleSetup = styleSetup;
            DataStore = dataStore;
            UserContext = userContext;
            HttpContextAccessor = httpContextAccessor;
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

            var user = await QBicUtils.GetLoggedInUserAsync(UserContext, HttpContextAccessor);

            var footer = "Printed by " + user.UserName + " on " + System.DateTime.Now.ToString("yyyy-MM-dd");
            document.SetDocumentFooter(footer);
            document.SetSideMargin(Unit.FromCentimeter(1));

            document.AddRowHeading("User\t\tDate Time");
            document.AddRowHeading("Entity\t\tAction");

            using (var session = DataStore.OpenAuditSession())
            {
                var auditEvents = session.CreateCriteria<AuditEvent>()
                                         .Add(Restrictions.Gt("AuditEventDateTimeUTC", fromDate.AddDays(-1)))
                                         .Add(Restrictions.Lt("AuditEventDateTimeUTC", toDate.AddDays(1)))
                                         .Add(Restrictions.In("UserId", userIds))
                                         .AddOrder(Order.Asc("AuditEventDateTimeUTC"))
                                         .AddOrder(Order.Asc("EntityName"))
                                         .List<AuditEvent>()
                                         .ToList();

                var jdp = new JsonDiffPatch();
                
                foreach (var audit in auditEvents)
                {
                    if (audit.OriginalObject == null && audit.NewObject == null)
                    {
                        continue;
                    }
                    var origObj = audit.OriginalObject ?? String.Empty;
                    var newObj = audit.NewObject.Replace(",\"", ", \"");

                    var diff = String.Empty;

                    if (String.IsNullOrWhiteSpace(audit.OriginalObject))
                    {
                        diff = audit.NewObject;
                    }
                    else if (String.IsNullOrWhiteSpace(audit.NewObject))
                    {
                        diff = audit.OriginalObject;
                    }
                    else
                    {
                        var left = JToken.Parse(audit.OriginalObject);
                        var right = JToken.Parse(audit.NewObject);

                        var patch = jdp.Diff(left, right);
                        diff = jdp.Patch(left, patch)?.ToString();
                        //var diffItem = jdp.Diff(left, right);
                        //diff = diffItem.ToString();
                    }

                    //document.AddRowUsingParams(audit.User.UserName + "\t" + audit.AuditEventDateTimeUTC.ToString() + "\t" + audit.AuditAction.ToString() + "\t\t" + audit.EntityName + "\n" + origObj, "\n" + newObj + "\n\n" + diff);
                    document.AddRowUsingParams(audit.UserName + "\t\t" + audit.AuditEventDateTimeUTC.ToString(), audit.EntityName + "\t\t" + audit.AuditAction.ToString());
                    document.AddRowUsingParams("ORIGINAL ITEM", "MODIFICATION");
                    document.AddRowUsingParams(origObj, diff);
                    document.AddEmptyRow();
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

        public override string GetFileNameAndExtension()
        {
            return "Audit report.pdf";
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