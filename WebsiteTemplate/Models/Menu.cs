using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Models
{
    public class Menu: BaseClass
    {
        public Menu()
        {
            AllowedUserRoles = new List<UserRole>();
        }

        public virtual string Name { get; set; }

        //private string mUserRoleString { get; set; }
        public virtual string UserRoleString
        {
            get
            {
                //mUserRoleString = String.Join(",", AllowedUserRoles.ToArray());
                return String.Join(",", AllowedUserRoles.ToArray()) ?? String.Empty;
            }
            set
            {
                //mUserRoleString = value;
                AllowedUserRoles = value.Split(",".ToCharArray()).Select(u => (UserRole)Enum.Parse(typeof(UserRole), u)).ToList();
            }
        }

        public List<UserRole> AllowedUserRoles { get; set; }

        public virtual Menu ParentMenu { get; set; }

        public virtual EventNumber? Event { get; set; }
    }
}