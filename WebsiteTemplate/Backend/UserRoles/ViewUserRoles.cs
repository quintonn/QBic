using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class ViewUserRoles : ShowView
    {
        private UserRoleService UserRoleService { get; set; }

        public ViewUserRoles(UserRoleService service)
        {
            UserRoleService = service;
        }
        public override string Description
        {
            get
            {
                return "View User Roles";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Description", "Description");

            columnConfig.AddLinkColumn("", "Id", "Edit", EventNumber.EditUserRole);

            columnConfig.AddButtonColumn("", "Id", "X",
                new UserConfirmation("Delete User Role?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteUserRole
                }
            );
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            return UserRoleService.RetrieveUserRoles(currentPage, linesPerPage, filter);
        }

        public override int GetDataCount(string data, string filter)
        {
            return UserRoleService.RetrieveUserRoleCount(filter);
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewUserRoles;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var results = new List<MenuItem>();
            results.Add(new MenuItem("Add", EventNumber.AddUserRole));

            return results;
        }
    }
}