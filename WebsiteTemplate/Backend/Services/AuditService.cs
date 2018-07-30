using QBic.Core.Data;
using QBic.Core.Models;
using NHibernate;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class AuditService
    {
        private DataStore DataStore { get; set; }
        private UserContext UserContext { get; set; }

        private ApplicationSettingsCore AppSettings { get; set; }
        public AuditService(DataStore dataStore, UserContext userContext, ApplicationSettingsCore appSettings)
        {
            DataStore = dataStore;
            UserContext = userContext;
            AppSettings = appSettings;
        }

        public async Task LogUserEvent(int eventId)
        {
            //var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync() as User;

            //TODO
            // May have to also create a web call for user confirmation screens/dialogs/notifications.
            // This is to log when a user clicks ok on a message dialog box and log what the user chose and the message shown

            // I think this logging should relay some kind of info like this:
            // User X click on menu/button/link for ABC
            // Eg. User X clicked delete on "Menus" item "View Users"
            //     User X clicked on "Ok" on message "Menu Deleted successfully"
            //     User X clicked on menu item "Show Claims"
            //     User X clicked on "Cancel" on "Edit Cause" of "Test Cause"   --might have to show Id
            // I think this covers everything
        }

        public void AuditChange<T>(ISession session, string itemId, T item, AuditAction action, string entityName, User user = null) where T : BaseClass
        {
            //return; // Globally disable auditing for now because it does not work properly.
            if (AppSettings.EnableAuditing == false)
            {
                return;
            }

            if (user == null)
            {
                var userTask = BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync(UserContext);
                userTask.Wait();
                user = userTask.Result as User;
            }
            if (user == null)
            {
                throw new Exception("Null user when trying to perform audit");
            }

            entityName = entityName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();

            //if (existingItem == null)
            //{
            //    existingItem = item;
            //}

            using (var auditSession = DataStore.OpenAuditSession())
            using (var statelessSession = DataStore.OpenStatelessSession())
            {
                string originalObject = null;
                if (!String.IsNullOrWhiteSpace(itemId))
                {
                    var dbItem = statelessSession.Get<T>(itemId); // Get original/unmodified item
                    originalObject = SerializeObject(dbItem);
                }
                var auditEvent = new AuditEvent()
                {
                    AuditAction = action,
                    AuditEventDateTimeUTC = DateTime.UtcNow,
                    UserId = user.Id,
                    UserName = user.UserName,
                    ObjectId = item.Id,
                    EntityName = entityName,
                    OriginalObject = originalObject,
                    NewObject = action != AuditAction.Delete ? SerializeObject(item) : String.Empty
                };
                auditSession.Save(auditEvent);

                auditSession.Flush();
            }
        }

        private string SerializeObject(object item)
        {
            if (item == null)
            {
                return String.Empty;
            }
            var result = JsonHelper.SerializeObject(item, true);
            return result;
        }
    }
}