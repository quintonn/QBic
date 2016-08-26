using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

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

        public void AuditChange<T>(T item, AuditAction action) where T : BaseClass
        {
            var id = item.Id;
        }
    }
}