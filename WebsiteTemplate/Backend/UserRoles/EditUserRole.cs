using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class EditUserRole : ModifyUserRole
    {
        public EditUserRole(UserRoleService service, UserRoleProcessor processor, DataService dataService)
            : base(processor, service, false, dataService)
        {
        }

        public override EventNumber ViewToShowAfterModify
        {
            get
            {
                return EventNumber.ViewUserRoles;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.EditUserRole;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
    }
}