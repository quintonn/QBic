using QBic.Core.Data;
using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QBic.Core.Mappings
{
    public static class BaseMap
    {
        //protected string TableName = typeof(T).Name.Split(".".ToCharArray()).Last();

        //protected IList<PropertyInfo> Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
        //                                                    .Where(p => p.GetMethod.IsVirtual && p.GetMethod.IsAbstract == false)
        //                                                    .ToList();

        public static void MapPrimitiveTypes<T>(this ClasslikeMapBase<T> dynamicMap, IList<string> primitiveColumns, IEnumerable<PropertyInfo> properties) where T : DynamicClass
        {
            foreach (var column in primitiveColumns)
            {
                if (column == "CanDelete" || column == "Id")
                {
                    continue;
                }

                var propertyType = properties.Where(p => p.Name == column).Single().PropertyType;

                if (propertyType == typeof(byte[]))
                {
                    //if (DataStore.ProviderName.Contains("MySql"))
                    //{
                    //    dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().CustomSqlType("LONGBLOB").Length(int.MaxValue);
                    //}
                    if (DataStore.SetCustomSqlTypes == true)
                    {
                        dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().CustomSqlType("varbinary(max)").Length(int.MaxValue);
                    }
                    else
                    {
                        dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable().Length(int.MaxValue);
                    }
                }
                else if (propertyType == typeof(LongString))
                {
                    //if (DataStore.ProviderName.Contains("MySql"))
                    //{
                    //    dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomType<LongString>().CustomSqlType("LONGTEXT").Length(int.MaxValue);
                    //}
                    if (DataStore.SetCustomSqlTypes == true)
                    {
                        dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomType<LongString>().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
                    }
                    else
                    {
                        dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomType<LongString>().Length(int.MaxValue);
                    }
                }
                else if (IsNullable(propertyType))
                {
                    dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable();
                }
                else
                {
                    dynamicMap.Map(FluentNHibernate.Reveal.Member<T>(column)).Not.Nullable();
                }
            }
        }

        public static bool IsNullable(Type type)
        {
            //if (!type.IsValueType) return true; // ref-type  this includes types like string
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static void MapNonPrimitiveTypes<T>(this ClasslikeMapBase<T> dynamicMap, IList<PropertyInfo> nonPrimitiveColumns, bool nullableReference) where T : DynamicClass
        {
            foreach (var column in nonPrimitiveColumns)
            {
                var types = new List<Type>();
                types.Add(typeof(T));
                types.Add(column.PropertyType);

                var method = typeof(FluentNHibernate.Reveal).GetMethods().Where(m => m.Name == "Member").Last();
                var generic = method.MakeGenericMethod(typeof(T), column.PropertyType);

                dynamic tmp = generic.Invoke(dynamicMap, new object[] { column.Name });

                if (nullableReference == false)
                {
                    dynamicMap.References(tmp)
                               .Not.Nullable()
                               .NotFound.Ignore()
                               .LazyLoad(Laziness.False);
                }
                else
                {
                    //TODO: Should this be nullable? Need to find examples of when it comes here.
                    dynamicMap.References(tmp)
                           //.Not.Nullable()
                           .Nullable()
                           .Cascade.None()
                           .NotFound.Ignore()
                           .LazyLoad(Laziness.False);
                }
            }
        }

        public static void MapLists<T>(this ClasslikeMapBase<T> dynamicMap, IList<PropertyInfo> listColumns) where T : DynamicClass
        {
            var tableName = typeof(T).Name.Split(".".ToCharArray()).Last();

            foreach (var column in listColumns)
            {
                var method = typeof(FluentNHibernate.Reveal).GetMethods().Where(m => m.Name == "Member").Last();

                var genericType = column.PropertyType.GenericTypeArguments.First();

                var listType = typeof(IEnumerable<>).MakeGenericType(new[] { genericType });
                var generic = method.MakeGenericMethod(typeof(T), listType);

                dynamic tmp = generic.Invoke(dynamicMap, new object[] { column.Name });

                //HasMany<object>(x => x.Id).KeyColumn("").Inverse().AsSet();  // for intellisense

                dynamicMap.HasMany(tmp).KeyColumn(tableName + "_id").Inverse().AsSet().Not.LazyLoad();//.Cascade.None();
            }
        }

        public static List<PropertyInfo> GetParentProperties(Type parentType)
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

        public static bool IsGenericList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.Name == "IEnumerable")
                {
                    return true;
                }
                if (interfaceType.IsGenericType)
                {
                    if (interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        // if needed, you can also return the type used as generic argument
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsPrimitive(Type t)
        {
            return new[] {
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(System.Byte[]),
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
                typeof(DateTime?),
                typeof(LongString)
            }.Contains(t) || t.IsPrimitive || t.IsEnum;
        }
    }
}