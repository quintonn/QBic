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
            var tableName = typeof(T).Name.Split(".".ToCharArray()).Last();

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => p.GetMethod.IsVirtual)
                                                            .ToList();

            var parentProperties = GetParentProperties(typeof(T).BaseType).Select(p => p.Name)
                                                                          .ToList();

            properties = properties.Where(p => !parentProperties.Contains(p.Name)).ToList();

            var primitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false && IsGenericList(p.PropertyType) == false).ToList();
            var listColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false && IsGenericList(p.PropertyType) == true).ToList();

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

            foreach (var column in listColumns)
            {
                var method = typeof(FluentNHibernate.Reveal).GetMethods().Where(m => m.Name == "Member").Last();

                var genericType = column.PropertyType.GenericTypeArguments.First();

                //var listType = Type.GetType("System.Collections.Generic.IEnumerable<" + genericType + ">");
                var listType = typeof(IEnumerable<>).MakeGenericType(new[] { genericType });
                var generic = method.MakeGenericMethod(typeof(T), listType);

                dynamic tmp = generic.Invoke(this, new object[] { column.Name });

                //HasMany<object>(x => x.Id).KeyColumn("").Inverse().AsSet();  // for intellisense

                HasMany(tmp).KeyColumn(tableName + "_id").Inverse().AsSet().Not.LazyLoad();//.Cascade.None();
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

        private bool IsGenericList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.Name == "IEnumerable")
                {
                    return true;
                }
                if (@interface.IsGenericType)
                {
                    if (@interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        // if needed, you can also return the type used as generic argument
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsPrimitive(Type t)
        {
            return Utilities.XXXUtils.IsPrimitive(t);
        }
    }
}