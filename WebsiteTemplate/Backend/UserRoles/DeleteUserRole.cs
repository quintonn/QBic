using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
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

        public override int GetId()
        {
            return EventNumber.DeleteUserRole;
        }

        public override async Task<IList<Event>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            using (var session = Store.OpenSession())
            {
                var userRoleAssociations = session.CreateCriteria<UserRoleAssociation>()
                                                  .CreateAlias("UserRole", "role")
                                                  .Add(Restrictions.Eq("role.Id", id))
                                                  .List<UserRoleAssociation>()
                                                  .ToList();
                if (userRoleAssociations.Count > 0)
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Cannot delete user role, it is assigned to users.")
                    };
                }

                var eventRoles = session.CreateCriteria<EventRoleAssociation>()
                                        .CreateAlias("UserRole", "role")
                                        .Add(Restrictions.Eq("role.Id", id))
                                        .List<EventRoleAssociation>()
                                        .ToList();
                eventRoles.ForEach(e =>
                {
                    session.Delete(e);
                });

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