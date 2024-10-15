using QBic.Core.Models;
using FluentNHibernate.Mapping;
using QBic.Core.Data;

namespace QBic.Core.Mappings
{
    public class BaseClassMap<T> : ClassMap<T> where T : BaseClass
    {
        public BaseClassMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.CanDelete).Default(DataStore.GetDefaultBoolean())
                                 .Not.Nullable();
        }
    }
}