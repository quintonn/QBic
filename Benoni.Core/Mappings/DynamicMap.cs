using Benoni.Core.Models;
using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Benoni.Core.Mappings
{
    public class DynamicMap<T> : ClassMap<T> where T : DynamicClass
    {
        public string TableName = typeof(T).Name.Split(".".ToCharArray()).Last();
        private IList<PropertyInfo> Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => p.GetMethod.IsVirtual && !p.GetMethod.IsAbstract)
                                                            .ToList();

        public DynamicMap()
        {
            Table(TableName);

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

            var primitiveColumns = Properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = Properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == false && BaseMap.IsGenericList(p.PropertyType) == false).ToList();
            var listColumns = Properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == false && BaseMap.IsGenericList(p.PropertyType) == true).ToList();

            this.MapPrimitiveTypes(primitiveColumns, Properties);

            this.MapNonPrimitiveTypes(nonPrimitiveColumns, true);

            this.MapLists(listColumns);
        }
    }
}