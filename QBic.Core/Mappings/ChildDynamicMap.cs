﻿using FluentNHibernate.Mapping;
using QBic.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QBic.Core.Mappings
{
    public class ChildDynamicMap<T> : SubclassMap<T> where T : DynamicClass
    {
        private string TableName = typeof(T).Name.Split(".".ToCharArray()).Last();
        private IList<PropertyInfo> Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => p.GetMethod.IsVirtual && p.GetMethod.IsAbstract == false && p.GetSetMethod() != null)
                                                            .ToList();

        public ChildDynamicMap()
            :base()
        {
            var parentProperties = BaseMap.GetParentProperties(typeof(T).BaseType).Select(p => p.Name).ToList();

            var properties = Properties.Where(p => !parentProperties.Contains(p.Name)).ToList();

            var primitiveColumns = properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == false && BaseMap.IsGenericList(p.PropertyType) == false).ToList();
            var listColumns = properties.Where(p => BaseMap.IsPrimitive(p.PropertyType) == false && BaseMap.IsGenericList(p.PropertyType) == true).ToList();

            this.MapPrimitiveTypes(primitiveColumns, properties);

            this.MapNonPrimitiveTypes(TableName, nonPrimitiveColumns, false);

            this.MapLists(listColumns);
        }

       
    }
}