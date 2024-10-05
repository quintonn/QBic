using QBic.Core.Models;
using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QBic.Core.Mappings
{
    public class DynamicMap<T> : ClassMap<T> where T : DynamicClass
    {
        private string TableName = typeof(T).Name.Split(".".ToCharArray()).Last();
        private IList<PropertyInfo> Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => p.GetMethod.IsVirtual && !p.GetMethod.IsAbstract && p.GetSetMethod() != null)
                                                            .ToList();

        public DynamicMap()
        {
            Table(TableName);

            Id(x => x.Id).GeneratedBy.Assigned(); // This is for when doing backup restore.

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();

            var primitiveColumns = Properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = Properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == false && BaseMap.IsGenericList(p.PropertyType) == false).ToList();
            var listColumns = Properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == false && BaseMap.IsGenericList(p.PropertyType) == true).ToList();

            this.MapPrimitiveTypes(primitiveColumns, Properties);

            this.MapNonPrimitiveTypes(TableName, nonPrimitiveColumns, false);

            this.MapLists(listColumns);
        }
    }
}