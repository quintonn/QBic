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

        public override string Description
        {
            get
            {
                return "View Menus";
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
            columnConfig.AddStringColumn("Id", "Id");
            columnConfig.AddStringColumn("Name", "Name");
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
                }
                var results = query
                       //.Add(Restrictions.Eq("", ""))   //TODO: Can add filter/query items here
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