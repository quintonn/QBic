using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class ViewUserRoleAssociations : ShowView
    {
        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("User", "User.UserName");    /// User.UserName will be processed at runtime in javascript.
            columnConfig.AddStringColumn("User Role", "UserRoleString");
        }

        public override Type GetDataType()
        {
            return typeof(UserRoleAssociation);
        }

        public override Menus.BaseItems.EventNumber GetId()
        {
            return Menus.BaseItems.EventNumber.ViewUserRoleAssociations;
        }

        public override string Description
        {
            get
            {
                return "View User Role Associations";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.AnyOne
                };
            }
        }

        public override IList<MenuItem> ViewMenu
        {
            get
            {
                var results = new List<MenuItem>();
                results.Add(new MenuItem("Add", EventNumber.AddUserRoleAssociation));
                return results;
            }
        }


        public override string GetViewMessage()
        {
            return "TODO: Make this be not a menu item as such but a link from users screen";
        }
    }
}