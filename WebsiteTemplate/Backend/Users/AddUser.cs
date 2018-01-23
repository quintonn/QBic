using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Users
{
    public class AddUser : ModifyUser
    {
        public AddUser(UserProcessor userProcessor, UserService userService, UserInjector injector)
            : base(userService, userProcessor, injector, true)
        {
        }

        public override EventNumber GetId()
        {
            return EventNumber.AddUser;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }
    }
}