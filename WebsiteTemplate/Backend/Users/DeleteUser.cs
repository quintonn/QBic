using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;


namespace WebsiteTemplate.Backend.Users
{
    public class DeleteUser : DoSomething
    {
        public override EventNumber GetId()
        {
            return EventNumber.DeleteUser;
        }

        public override string Description
        {
            get
            {
                return "Deletes a user";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }

        public override async Task<IList<Event>> ProcessAction(string data)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var id = parameters["Id"];

            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(id);
                session.Delete(user);
                session.Flush();
            }

            return new List<Event>()
            {
                new ShowMessage("User deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewUsers, String.Empty)
            };
        }
    }
}