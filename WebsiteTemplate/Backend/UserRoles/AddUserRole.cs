﻿using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class AddUserRole : ModifyUserRole
    {
        public AddUserRole(UserRoleService service, UserRoleProcessor processor, DataService dataService)
            : base(processor, service, true, dataService)
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
            return EventNumber.AddUserRole;
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