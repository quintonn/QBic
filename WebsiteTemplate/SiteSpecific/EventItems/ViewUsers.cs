using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class ViewUsers : ShowView
    {
        public override EventNumber GetId()
        {
            return EventNumber.ViewUsers;
        }

        public override Type GetDataType()
        {
            return typeof(User);
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Id", "Id");
            columnConfig.AddStringColumn("Name", "UserName");
            columnConfig.AddStringColumn("Email", "Email");
            columnConfig.AddBooleanColumn("Email Confirmed", "EmailConfirmed", "Yes", "No");
            columnConfig.AddLinkColumn("", "Confirm Email", "Id", "Send Confirmation Email", EventNumber.SendConfirmationEmail,
                new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Show,
                    OtherColumnToCheck = "EmailConfirmed",
                    OtherColumnValue = "true"
                }
            );
            columnConfig.AddButtonColumn("", "", ButtonTextSource.Fixed, "X",
                columnSetting: new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Hide,
                    OtherColumnToCheck = "CanDelete",
                    OtherColumnValue = "true"
                },
                eventItem: new UserConfirmation("Delete User?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteUser
                }
            );
            columnConfig.AddLinkColumn("", "Edit", "Id", "Edit", EventNumber.EditUser);

            //columnConfig.AddLinkColumn("Associated Roles", "Roles", "Id", "...", EventNumber.EditUser);
            columnConfig.AddButtonColumn("Roles", "", ButtonTextSource.Fixed, "...",
                columnSetting: null,
                eventItem: new ExecuteAction(EventNumber.ViewUserRoleAssociations)
            );
        }

        public override IList<MenuItem> ViewMenu
        {
            get
            {
                var results = new List<MenuItem>();
                results.Add(new MenuItem("Add", EventNumber.AddUser));
                return results;
            }
        }

        public override string Description
        {
            get
            {
                return "View Users";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.ViewUsers
                };
            }
        }

        public override string GetViewMessage()
        {
            return "TODO: Need to add ability to limit number of results as well as ability to sort items\n" +
                   "ALSO: Will need to limit display to 100% of height and put table inside a scroller\n";
        }

        public override IEnumerable GetData(string data)
        {
            using (var session = Store.OpenSession())
            {
                var results = session.CreateCriteria<User>()
                    //.Add(Restrictions.Eq("", ""))   //TODO: Can add filter/query items here
                       .List<User>()
                       .ToList();
                return results;
            }
        }
    }
}