using System;
using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using System.Linq;
using NHibernate.Criterion;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class ViewMenus : ShowView
    {
        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>() { UserRole.AnyOne };
            }
        }

        private string mDescription = "View Menus";
        public override string Description
        {
            get
            {
                return mDescription;
            }
        }

        public override IList<MenuItem> ViewMenu
        {
            get
            {
                return new List<MenuItem>();
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Allowed Roles", "UserRoleString");
            columnConfig.AddStringColumn("Event", "Event", new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Hide,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.Equals, "")
                }
            });

            columnConfig.AddButtonColumn("Sub Menus", "", ButtonTextSource.Fixed, "...", new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Show,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.Equals, ""),
                    new Condition("ParentMenu", Comparison.Equals, "")
                }
            }, new ExecuteAction(EventNumber.ViewMenus));
        }

        public override IEnumerable GetData(string data)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.CreateCriteria<Menu>();
                if (!String.IsNullOrWhiteSpace(data))
                {
                    query = query.CreateAlias("ParentMenu", "parent")
                                 .Add(Restrictions.Eq("parent.Id", data));

                    var parentMenu = session.Get<Menu>(data);
                    mDescription = "View Sub-menus: " + parentMenu.Name;
                }
                else
                {
                    mDescription = "View Menus";
                    query = query.Add(Restrictions.IsNull("ParentMenu"));
                }
                var results = query
                       .List<Menu>()
                       .ToList();
                return results;
            }
        }

        public override Type GetDataType()
        {
            return typeof(Menu);
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewMenus;
        }
    }
}