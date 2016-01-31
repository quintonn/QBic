using Newtonsoft.Json;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using System.Linq;


namespace WebsiteTemplate.Backend.Users
{
    public class DeleteUser : DoSomething
    {
        public override int GetId()
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

        public override async Task<IList<Event>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(id);

                var userRoles = session.CreateCriteria<UserRoleAssociation>()
                                       .CreateAlias("User", "user")
                                       .Add(Restrictions.Eq("user.Id", id))
                                       .List<UserRoleAssociation>()
                                       .ToList();
                userRoles.ForEach(u =>
                {
                    session.Delete(u);
                });

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