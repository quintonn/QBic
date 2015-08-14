using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class ViewUserRoleAssociations : ShowView
    {
        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("User", "User.UserName");
            columnConfig.AddStringColumn("User Role", "UserRoleString");
        }

        public override Type GetDataType()
        {
            return typeof(UserRoleAssociation);
        }

        public override IList<Menus.MenuItem> ViewMenu
        {
            get
            {
                return new List<MenuItem>();
            }
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

        public override string ViewMessage
        {
            get
            {
                return "TODO: Create some kind of table for menus and users allowed to see them";
            }
        }
    }
}