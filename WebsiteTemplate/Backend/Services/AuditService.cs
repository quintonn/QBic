using Microsoft.Practices.Unity;
using System;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class AuditService
    {
        private DataStore DataStore { get; set; }
        public AuditService(DataStore dataStore)
        {
            DataStore = dataStore;
        }

        public async Task LogUserEvent(int eventId)
        {
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync() as User;
        }

        public async void AuditChange<T>(T item, AuditAction action) where T : BaseClass
        {
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync() as User;

            using (var session = DataStore.OpenSession())
            {
                object existingItem = null;
                if (!String.IsNullOrWhiteSpace(item.Id))
                {
                    existingItem = session.Get<T>(item.Id);
                }

                var auditEvent = new AuditEvent()
                {
                    AuditAction = action,
                    AuditEventDateTimeUTC = DateTime.UtcNow,
                    User = user,
                    ObjectId = item.Id,
                    OriginalObject = SerializeObject(existingItem),
                    NewObject = action != AuditAction.Delete ? SerializeObject(item) : String.Empty
                };
                session.Save(auditEvent);
                session.Flush();
            }
        }

        private string SerializeObject(object item)
        {
            if (item == null)
            {
                return String.Empty;
            }
            var result = JsonHelper.SerializeObject(item);
            return result;
        }
    }
}