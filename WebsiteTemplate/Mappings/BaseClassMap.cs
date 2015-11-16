using FluentNHibernate.Mapping;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class BaseClassMap<T> : ClassMap<T> where T : BaseClass
    {
        public BaseClassMap()
        {
            Id(x => x.Id).GeneratedBy.Custom<CustomIdentifierGenerator>();

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();
        }
    }
}