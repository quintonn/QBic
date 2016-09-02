﻿using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Users
{
    public class DeleteUser : DeleteItemUsingDataService<UserProcessor, User>
    {
        public DeleteUser(UserProcessor userProcessor)
            : base(userProcessor)
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
    }
}