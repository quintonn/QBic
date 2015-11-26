using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class DeleteUserRole : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Delete User Role";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteUserRole;
        }

        public override async Task<IList<Event>> ProcessAction(string data)
        {
            var json = JObject.Parse(data);

            var id = json.GetValue("Id").ToString();

            using (var session = Store.OpenSession())
            {
                var userRole = session.Get<UserRole>(id);
                session.Delete(userRole);
                session.Flush();
            }

            return new List<Event>()
            {
                new ShowMessage("User role deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewUserRoles)
            };
        }
    }
}