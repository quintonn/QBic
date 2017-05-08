using FluentNHibernate.Mapping;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class MenuMap : BaseClassMap<Menu>
    {
        public MenuMap()
        {
            Table("Menu");

            References(x => x.ParentMenu).Column("IdParentMenu")
                           .Nullable()
                           .LazyLoad(Laziness.False);

            Map(x => x.Name).Not.Nullable();

            //HasMany<Menu>(x => x.SubMenus).KeyColumn("Id").Table("Menu").AsList();

            Map(x => x.Event)
              //.CustomType<EventNumber>()
              //.Not
              .Nullable();

            Map(x => x.Position)
                .Default("-1")
                .Not.Nullable();
        }
    }
}