using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            var primitiveColumnsTo = properties.Where(p => IsPrimitive(p.PropertyType) == true).Select(p => p.Name).ToList();
            var nonPrimitiveColumns = properties.Where(p => IsPrimitive(p.PropertyType) == false).ToList();

            foreach (var column in primitiveColumnsTo)
            {
                if (column == "CanDelete" || column == "Id")
                {
                    continue;
                }

                Map(FluentNHibernate.Reveal.Member<T>(column))
                    //.Not.Nullable();
                    .Nullable();
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
                           //.Not.Nullable()
                           .Nullable()
                           .LazyLoad(Laziness.False);
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