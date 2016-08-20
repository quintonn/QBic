using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class AuditService
    {
        private DataStore Store { get; set; }

        public AuditService(DataStore store)
        {
            Store = store;
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

    public enum AuditAction
    {
        New = 0,
        Modify = 1,
        Delete = 2
    }
}