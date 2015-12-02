using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class ViewUserRoles : ShowView
    {
        public override string Description
        {
            get
            {
                return "User Roles";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Description", "Description");

            columnConfig.AddLinkColumn("", "Edit", "Id", "Edit", EventNumber.EditUserRole);

            columnConfig.AddButtonColumn("", "", ButtonTextSource.Fixed, "X", null,
                new UserConfirmation("Delete User Role?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteUserRole
                }
            );
        }

        public override IEnumerable GetData(string data)
        {
            using (var session = Store.OpenSession())
            {
                var results = session.CreateCriteria<UserRole>()
                                     .List<UserRole>()
                                     .ToList();
                return results;
            }
        }

        public override int GetId()
        {
            return EventNumber.ViewUserRoles;
        }

        public override IList<MenuItem> GetViewMenu()
        {
            var results = new List<MenuItem>();
            results.Add(new MenuItem("Add", EventNumber.AddUserRole));

            return results;
        }
    }
}