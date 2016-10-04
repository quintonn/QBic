using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

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

        public override bool AllowInMenu
        {
            get
            {
                return true;
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

        public override IEnumerable GetData(GetDataSettings settings)
        {
            return UserRoleService.RetrieveUserRoles(settings.CurrentPage, settings.LinesPerPage, settings.Filter);
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            return UserRoleService.RetrieveUserRoleCount(settings.Filter);
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