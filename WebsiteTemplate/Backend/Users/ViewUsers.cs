using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.Users
{
    public class ViewUsers : ShowView
    {
        public override int GetId()
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

        public override string GetViewMessage()
        {
            return "TODO: Need to add ability to limit number of results as well as ability to sort items\n" +
                   "ALSO: Will need to limit display to 100% of height and put table inside a scroller\n";
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            using (var session = Store.OpenSession())
            {
                var results = session.QueryOver<User>()
                                     .Skip((currentPage - 1) * linesPerPage)
                                     .Take(linesPerPage)
                                     .List<User>()
                                     .ToList();
                //var results = session.CreateCriteria<User>()
                //       //.Add(Restrictions.Eq("", ""))   //TODO: Can add filter/query items here
                //       .List<User>()
                //       .ToList();
                return results;
            }
        }

        public override int GetDataCount(string data, string filter)
        {
            using (var session = Store.OpenSession())
            {
                var results = session.QueryOver<User>()
                                     .RowCount();
                return results;
            }
        }
    }
}