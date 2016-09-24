using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class DynamicMap<T> : ClassMap<T> where T : DynamicClass
    {
        public DynamicMap()
        {
            var tableName = typeof(T).Name.Split(".".ToCharArray()).Last();
            Table(tableName);

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


            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Where(p => p.GetMethod.IsVirtual);

            var primitiveColumnsTo = properties.Where(p => IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false && IsGenericList(p.PropertyType) == false).ToList();
            var listColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false && IsGenericList(p.PropertyType) == true).ToList();

            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();

            foreach (var column in primitiveColumnsTo)
            {
                if (column == "CanDelete" || column == "Id")
                {
                    continue;
                }

                if (properties.Where(p => p.Name == column).Single().PropertyType == typeof(byte[]))
                {
                    Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable().CustomSqlType("varbinary(max)").Length(int.MaxValue);
                }
                else
                {
                    Map(FluentNHibernate.Reveal.Member<T>(column)).Nullable();
                }
            }

            foreach (var column in nonPrimitiveColumns)
            {
                var method = typeof(FluentNHibernate.Reveal).GetMethods().Where(m => m.Name == "Member").Last();
                var generic = method.MakeGenericMethod(typeof(T), column.PropertyType);

                dynamic tmp = generic.Invoke(this, new object[] { column.Name });

                References(tmp)
                           //.Not.Nullable()
                           .Nullable()
                           .Cascade.None()
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

        private static bool IsPrimitive(Type t)
        {
            // TODO: put any type here that you consider as primitive as I didn't
            // quite understand what your definition of primitive type is
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
                typeof(DateTime?)
            }.Contains(t) || t.IsPrimitive || t.IsEnum;
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
    }
}