using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Users
{
    public class EditUser : ModifyUser
    {
        public EditUser(UserProcessor userProcessor, UserService userService)
            : base(userService, userProcessor, false)
        {
        }
        
        public override EventNumber GetId()
        {
            return EventNumber.EditUser;
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