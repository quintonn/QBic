using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override Type GetDataType()
        {
            return typeof(UserRoleAssociation);
        }

        public override Menus.BaseItems.EventNumber GetId()
        {
            return Menus.BaseItems.EventNumber.ViewUserRoleAssociations;
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
                results.Add(new MenuItem("Add", EventNumber.AddUserRoleAssociation, String.Empty));
                return results;
            }
        }

        public override System.Collections.IEnumerable GetData(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(data, "Cannot show view of user role without data");
            }
            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(data);
                mDescription = "View User Roles: " + user.UserName;
                                
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