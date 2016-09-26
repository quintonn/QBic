using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class ChildDynamicMap<T> : SubclassMap<T>  where T : DynamicClass
    {
        public ChildDynamicMap()
        {
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => p.GetMethod.IsVirtual)
                                                            .ToList();

            var parentProperties = GetParentProperties(typeof(T).BaseType).Select(p => p.Name)
                                                                          .ToList();

            properties = properties.Where(p => !parentProperties.Contains(p.Name)).ToList();

            var primitiveColumnsTo = properties.Where(p => IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false).ToList();

            foreach (var column in primitiveColumnsTo)
            {
                if (column == "CanDelete" || column == "Id")
                {
                    continue;
                }

                if (properties.Where(p => p.Name == column).Single().PropertyType == typeof(byte[]))
                {
                    if (DataStore.SetCustomSqlTypes == true)
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().CustomSqlType("varbinary(max)").Length(int.MaxValue);
                    }
                    else
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().Length(int.MaxValue);
                    }
                }
                else
                {
                    Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable();
                }
            }

            foreach (var column in nonPrimitiveColumns)
            {
                var types = new List<Type>();
                types.Add(typeof(T));
                types.Add(column.PropertyType);

                var method = typeof(FluentNHibernate.Reveal).GetMethods().Where(m => m.Name == "Member").Last();
                var generic = method.MakeGenericMethod(typeof(T), column.PropertyType);

                dynamic tmp = generic.Invoke(this, new object[] { column.Name });

                References(tmp)
                           .Not.Nullable()
                           .LazyLoad(Laziness.False);
            }
        }

        public List<PropertyInfo> GetParentProperties(Type parentType)
        {
            if (parentType == typeof(DynamicClass))
            {
                return new List<PropertyInfo>();
            }
            var properties = parentType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                   .Where(p => p.GetMethod.IsVirtual)
                                                   .ToList();
            var moreParentProperties = GetParentProperties(parentType.BaseType);
            properties.AddRange(moreParentProperties);

            return properties;
        }

        private static bool IsPrimitive(Type t)
        {
            // TODO: put any type here that you consider as primitive as I didn't
            // quite understand what your definition of primitive type is
            return new[] {
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(sbyte),
                typeof(ushort),
                typeof(short),
                typeof(uint),
                typeof(int),
                typeof(ulong),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
            }.Contains(t) || t.IsPrimitive || t.IsEnum;
        }
    }
}