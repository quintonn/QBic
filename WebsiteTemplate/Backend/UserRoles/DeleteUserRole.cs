using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

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

        private UserRoleService UserRoleService { get; set; }

        public DeleteUserRole(UserRoleService service)
        {
            UserRoleService = service;
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteUserRole;
        }

        public override async Task<IList<Event>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            using (var session = Store.OpenSession())
            {
                var isAssigned = UserRoleService.UserRoleIsAssigned(id);
                if (isAssigned)
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Cannot delete user role, it is assigned to users.")
                    };
                }

                UserRoleService.DeleteUserRole(id);
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