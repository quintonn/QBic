using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class DeleteUserRoleAssociation : DoSomething
    {
        public override EventNumber GetId()
        {
            return EventNumber.DeleteUserRoleAssociation;
        }

        public override string Description
        {
            get
            {
                return "Deletes user role";
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
            try
            {
                dynamic tmp = JsonConvert.DeserializeObject(data);
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                var id = parameters["Id"];

                try
                {
                    using (var session = Store.OpenSession())
                    {
                        var user = session.Get<UserRoleAssociation>(id);
                        session.Delete(user);
                        session.Flush();
                    }
                }
                catch (Exception eee)
                {
                    return new List<Event>()
                {
                    new ShowMessage(eee.Message)
                };
                }

                return new List<Event>()
                {
                    new ShowMessage("User role deleted successfully"),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoleAssociations)
                };
            }
            catch (Exception eee)
            {
                return null;
            }
        }
    }
}