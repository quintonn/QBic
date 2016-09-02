using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class DeleteUserRole : DeleteItemUsingDataService<UserRoleProcessor, UserRole>
    {
        public override string Description
        {
            get
            {
                return "Delete User Role";
            }
        }

        public override EventNumber ViewToShowAfterModify
        {
            get
            {
                return EventNumber.ViewUserRoles;
            }
        }

        public DeleteUserRole(UserRoleProcessor processor)
            : base(processor)
        {
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteUserRole;
        }
    }
}