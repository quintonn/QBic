using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Backend.Users
{
    public class ViewUsers : ShowView
    {
        private UserService UserService { get; set; }

        public ViewUsers(UserService service)
        {
            UserService = service;
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewUsers;
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "UserName");
            columnConfig.AddStringColumn("Email", "Email");
            columnConfig.AddBooleanColumn("Email Confirmed", "EmailConfirmed", "Yes", "No");
            columnConfig.AddLinkColumn("", "Id", "Send Confirmation Email", EventNumber.SendConfirmationEmail,
                new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Hide,
                    Conditions = new List<Condition>()
                    {
                        new Condition("EmailConfirmed", Comparison.Equals, "true")
                    }
                }
            );
            
            columnConfig.AddLinkColumn("", "Id", "Edit", EventNumber.EditUser);
            columnConfig.AddLinkColumn("", "Id", "View", EventNumber.Test);

            //columnConfig.AddButtonColumn("Roles", "", ButtonTextSource.Fixed, "...",
            //    columnSetting: null,
            //    eventItem: new ExecuteAction(EventNumber.ViewUserRoleAssociations, String.Empty) /// The data is passed from the view automatically
            //);

            columnConfig.AddButtonColumn("", "Id", "X",
                new UserConfirmation("Delete User?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteUser
                },
                new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Show,
                    Conditions = new List<Condition>()
                   {
                       new Condition("CanDelete", Comparison.Equals, "true")
                   }
                }
            );
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var results = new List<MenuItem>();
            results.Add(new MenuItem("Add", EventNumber.AddUser));
            return results;
        }

        public override string Description
        {
            get
            {
                return "View Users";
            }
        }

        /// <summary>
        /// Override this method to display a message below the list of data on a view.
        /// </summary>
        /// <returns></returns>
        public override string GetViewMessage()
        {
            return "TODO: Ability to sort items";
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            return UserService.RetrieveUsers(CurrentPage, linesPerPage, filter);
        }

        public override int GetDataCount(string data, string filter)
        {
            return UserService.RetrieveUserCount(filter);
        }
    }
}