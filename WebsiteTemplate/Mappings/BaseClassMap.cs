using FluentNHibernate.Mapping;
using QBic.Core.Models;

namespace WebsiteTemplate.Mappings
{
    public class BaseClassMap<T> : ClassMap<T> where T : BaseClass
    {
        public BaseClassMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();
        }
    }
}