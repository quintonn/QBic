using System.Collections.Generic;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Users
{
    public class ViewUsers : ShowViewUsingDataService<UserProcessor, User>
    {
        public ViewUsers(UserProcessor userProcessor)
            : base(userProcessor)
        {

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
    }
}