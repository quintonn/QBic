using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class AddUser : GetInput
    {
        public override EventNumber Id
        {
            get
            {
                return EventNumber.AddUser;
            }
        }

        public override string Name
        {
            get
            {
                return "Add User";
            }
        }

        public override string Description
        {
            get
            {
                return "Add a new user";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Add";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.AddUser
                };
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("UserName", "User Name"));
                list.Add(new StringInput("Email", "Email"));
                list.Add(new PasswordInput("Password", "Password"));
                list.Add(new PasswordInput("ConfirmPassword", "Confirm Password"));

                return list;
            }
        }

        public override IList<Menus.BaseItems.Event> InputButtons
        {
            get
            {
                return new List<Event>()
                {
                    new CreateUser(),
                    new CancelInputDialog()
                };
            }
        }
    }
}