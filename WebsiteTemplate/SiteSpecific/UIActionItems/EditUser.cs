using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class EditUser : DoSomething
    {
        public override async System.Threading.Tasks.Task<IList<Menus.BaseItems.Event>> ProcessAction(string data)
        {
            //var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            //var id = parameters["Id"];
            var id = data;
            var results = new List<Event>();
            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(id);

                results.Add(new DoEditUser(user));

                session.Flush();
            }

            return results;
        }

        public override EventNumber Id
        {
            get
            {
                return EventNumber.EditUser;
            }
        }

        public override string Name
        {
            get
            {
                return "Edit User";
            }
        }

        public override string Description
        {
            get
            {
                return "Edit User";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "";
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