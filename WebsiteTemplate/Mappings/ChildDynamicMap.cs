using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Data;
using WebsiteTemplate.Data.BaseTypes;
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

            var primitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false).ToList();

            foreach (var column in primitiveColumns)
            {
                if (column == "CanDelete" || column == "Id")
                {
                    continue;
                }

                if (properties.Where(p => p.Name == column).Single().PropertyType == typeof(byte[]))
                {
                    if (DataStore.ProviderName.Contains("MySql"))
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().CustomSqlType("LONGBLOB").Length(int.MaxValue);
                    }
                    else if (DataStore.SetCustomSqlTypes == true)
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().CustomSqlType("varbinary(max)").Length(int.MaxValue);
                    }
                    else
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().Length(int.MaxValue);
                    }
                }
                else if (properties.Where(p => p.Name == column).Single().PropertyType == typeof(LongString))
                {
                    if (DataStore.ProviderName.Contains("MySql"))
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomType<LongString>().CustomSqlType("LONGTEXT").Length(int.MaxValue);
                    }
                    else if (DataStore.SetCustomSqlTypes == true)
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomType<LongString>().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
                    }
                    else
                    {
                        Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomType<LongString>().Length(int.MaxValue);
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
                           .NotFound.Ignore()
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
            return Utilities.XXXUtils.IsPrimitive(t);
        }
    }
}