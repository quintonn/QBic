using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class ProcessDoEditUser : DoSomething
    {
        public ProcessDoEditUser()
        {

        }

        public override async System.Threading.Tasks.Task<IList<Menus.BaseItems.Event>> ProcessAction(string data)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            using (var session = Store.OpenSession())
            {
                var id = parameters["Id"];
                var dbUser = session.Get<User>(id);
                dbUser.UserName = parameters["UserName"];
                dbUser.Email = parameters["Email"];
                session.Update(dbUser);
                session.Flush();
            }

            return new List<Event>()
            {
                new ShowMessage("User modified successfully."),
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewUsers)
            };
        }

        public override EventNumber Id
        {
            get
            {
                return EventNumber.ProcessEditUser;
            }
        }

        public override string Name
        {
            get
            {
                return "";
            }
        }

        public override string Description
        {
            get
            {
                return "";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Submit";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }
    }
}