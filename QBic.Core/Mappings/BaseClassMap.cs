using QBic.Core.Models;
using FluentNHibernate.Mapping;

namespace QBic.Core.Mappings
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