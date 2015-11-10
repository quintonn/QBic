using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class MenuMap : BaseClassMap<Menu>
    {
        public MenuMap()
        {
            Table("Menus");

            References(x => x.ParentMenu).Column("IdParentMenu")
                           .Nullable()
                           .LazyLoad(Laziness.False);

            Map(x => x.Name).Not.Nullable();

            Map(x => x.UserRoleString).Not.Nullable().Length(10000);

            Map(x => x.Event)
              .CustomType<EventNumber>()
              .Nullable();
        }
    }
}