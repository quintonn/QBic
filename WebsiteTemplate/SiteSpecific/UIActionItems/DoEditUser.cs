using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class DoEditUser : AddUser
    {
        public DoEditUser()
        {

        }

        public DoEditUser(User user)
        {
            User = user;
        }

        public User User { get; set; }

        public override string Description
        {
            get
            {
                return "Edit User";
            }
        }

        public override int Id
        {
            get
            {
                return UIActionNumbers.DO_EDIT_USER;
            }
        }

        public override IList<Menus.BaseItems.UIAction> InputButtons
        {
            get
            {
                return new List<UIAction>()
                {
                    new ProcessDoEditUser(),
                    new CancelInputDialog()
                };
            }
        }
        public override IList<InputField> InputFields
        {
            get
            {
                var results = new List<InputField>();
                
                results.Add(new StringInput("UserName", "User Name", User.UserName));
                results.Add(new StringInput("Email", "Email", User.Email));
                results.Add(new HiddenInput("Id", User.Id));
                //results.Add(new PasswordInput("Password", "Password", User.pa));
                //results.Add(new PasswordInput("ConfirmPassword", "Confirm Password"));

                return results;
            }
        }
    }
}