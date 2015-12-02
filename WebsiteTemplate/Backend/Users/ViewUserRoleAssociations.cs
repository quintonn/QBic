using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.Users
{
    public class ViewUserRoleAssociations : ShowView
    {
        public string UserId { get; set; }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("User", "User.UserName");    /// User.UserName will be processed at runtime in javascript.
            columnConfig.AddStringColumn("User Role", "UserRole.Name");

            columnConfig.AddButtonColumn("", "", ButtonTextSource.Fixed, "X",
               columnSetting: new ShowHideColumnSetting()
               {
                   Display = ColumnDisplayType.Show,
                   Conditions = new List<Condition>()
                   {
                       new Condition("CanDelete", Comparison.Equals, "true")
                   }
               },
               eventItem: new UserConfirmation("Delete User Role?")
               {
                   OnConfirmationUIAction = EventNumber.DeleteUserRoleAssociation
               }
           );
        }

        public override int GetId()
        {
            return EventNumber.ViewUserRoleAssociations;
        }

        private string mDescription { get; set; }
        public override string Description
        {
            get
            {
                //return "View User Role Associations";
                return mDescription;
            }
        }

        public override IList<MenuItem> GetViewMenu()
        {
            var results = new List<MenuItem>();
            results.Add(new MenuItem("Add", EventNumber.AddUserRoleAssociation, UserId));
            return results;
        }
        public override System.Collections.IEnumerable GetData(string data)
        {
            UserId = data;

            if (String.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(data, "Cannot show view of user role without data");
            }
            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(data);
                mDescription = "User Roles: " + user.UserName;
                                
                var results = session.CreateCriteria<UserRoleAssociation>()
                       .CreateAlias("User", "user")
                       .Add(Restrictions.Eq("user.Id", data))
                       .List<UserRoleAssociation>()
                       .ToList();
                return results;
            }
        }
    }
}