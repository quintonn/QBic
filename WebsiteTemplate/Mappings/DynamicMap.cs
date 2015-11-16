using FluentNHibernate.Mapping;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    /// <summary>
    /// This class is so we can use reflection to find only the dynamic classes.
    /// This is used when dynamically mapping classes to NHibernate using FluentMapping.
    /// </summary>
    public class DynamicClass : BaseClass
    {
        
    }

    public class DynamicMap<T> : ClassMap<T> where T : DynamicClass
    {
        public DynamicMap()
        {
            var tableName = typeof(T).Name.Split(".".ToCharArray()).Last();
            Table(tableName);

            Id(x => x.Id).GeneratedBy.Custom<CustomIdentifierGenerator>();

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Where(p => p.GetMethod.IsVirtual);
            
            var columnsToAdd = properties.Select(p => p.Name).ToList();

            foreach (var column in columnsToAdd)
            {
                if (column == "CanDelete" || column == "Id")
                {
                    continue;
                }
                Map(FluentNHibernate.Reveal.Member<T>(column))
                    .Not.Nullable();
            }
        }
    }
}