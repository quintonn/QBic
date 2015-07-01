using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class ViewUsers : ShowView
    {
        public override int Id
        {
            get
            {
                return UIActionNumbers.VIEW_USERS;
            }
        }

        public override Type DataType
        {
            get
            {
                return typeof(User);
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Users";
            }
        }

        public override IList<ViewColumn> Columns
        {
            get
            {
                var results = new List<ViewColumn>();
                results.Add(new StringColumn("Id", "Id", "Id"));
                results.Add(new StringColumn("Name", "UserName", "UserName"));
                results.Add(new StringColumn("Email", "Email", "Email"));
                results.Add(new BooleanColumn("Email Confirmed", "EmailConfirmed", "EmailConfirmed", "Yes", "No"));
                results.Add(new LinkColumn("", "", "Confirm Email", "Id", "Send Confirmation Email", UIActionNumbers.SEND_CONFIRMATION_EMAIL)
                    {
                        ColumnSetting = new ShowHideColumnSetting()
                        {
                            Display = ColumnDisplayType.Show,
                            OtherColumnToCheck = "EmailConfirmed",
                            OtherColumnValue = "true"
                        }
                    });
                return results;
            }
        }

        public override IList<object> RowSettings
        {
            get
            {
                return new List<object>();
            }
            set
            {
                ;
            }
        }

        public override IList<object> OtherSettings
        {
            get
            {
                return new List<object>();
            }
            set
            {
                ;
            }
        }

        public override string Name
        {
            get
            {
                return "View Users";
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
    }
}