using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Users
{
    public class DeleteUser : DeleteItemUsingInputProcessor<UserProcessor, User>
    {
        public DeleteUser(UserProcessor userProcessor, DataService dataService)
            : base(userProcessor, dataService)
        {
        }

        public override string Description
        {
            get
            {
                return "Delete a user";
            }
        }

        public override EventNumber ViewToShowAfterModify
        {
            get
            {
                return EventNumber.ViewUsers;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteUser;
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