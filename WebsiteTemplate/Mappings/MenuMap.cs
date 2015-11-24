using FluentNHibernate.Mapping;
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

            Map(x => x.Event)
              .CustomType<EventNumber>()
              //.Not
              .Nullable();
        }
    }
}