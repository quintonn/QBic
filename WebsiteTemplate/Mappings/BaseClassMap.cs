using FluentNHibernate.Mapping;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class BaseClassMap<T> : ClassMap<T> where T : BaseClass
    {
        public BaseClassMap()
        {
            if (DynamicClass.SetIdsToBeAssigned == true)
            {
                Id(x => x.Id).GeneratedBy.Assigned(); // This is for when doing backup restore.
            }
            else
            {
                Id(x => x.Id).GeneratedBy.Custom<CustomIdentifierGenerator>();
            }

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();
        }
    }
}