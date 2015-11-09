using FluentNHibernate.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class TestClass
    {
        public TestClass()
        {
            Items = new Dictionary<string, string>();
            Items.Add("testColumn", "100");
            Items.Add("Namex", "x");
        }

        public virtual string testColumn
        {
            get
            {
                return Items["testColumn"];
            }
            set
            {
                Items["testColumn"] = value;
            }
        }

        public virtual string Id { get; set; }

        public virtual bool CanDelete { get; protected set; }

        public virtual Dictionary<string, string> Items { get; set; }
    }

    public class TestChildClass : TestClass
    {
        public virtual string Namex
        {
            get
            {
                return Items["Namex"];
            }
            set
            {
                Items["Namex"] = value;
            }
        }

        //public virtual string Namex { get; set; }
    }

    public class TestDynamicMap : ClassMap<TestChildClass>
    {
        public TestDynamicMap()
        {
            Table("TestTable");

            Id(x => x.Id).GeneratedBy.Custom<CustomIdentifierGenerator>();

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();


            var columnsToAdd = new List<string>()
            {
                "testColumn",
                "Namex"
            };

            var properties = typeof(TestChildClass).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Where(zz => zz.GetMethod.IsVirtual);
            
            columnsToAdd = properties.Select(p => p.Name).ToList();

            DynamicComponent(x => x.Items, dc =>
                {
                    
                    System.Diagnostics.Trace.WriteLine("Dynamic mapping");
                    foreach (var column in columnsToAdd)
                    {
                        Console.WriteLine(column);
                        System.Diagnostics.Trace.WriteLine(column + "----");

                        if (!column.Equals("testColumn", StringComparison.InvariantCultureIgnoreCase) && !column.Equals("Namex", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        
                        Console.WriteLine("Mapping " + column);
                        dc.Map<string>(column);
                        dc.Access.Property()
                             .Insert()
                             .Update()
                             .OptimisticLock()
                             .Unique();
                        //.ReadOnly();
                        dc.SqlDelete("SQL command");
                        dc.SqlDeleteAll("SQL command");
                        dc.SqlInsert("update testTable set testcolumn = 's'");
                        dc.SqlUpdate("SQL command");
                    }
                });

        }
    }
}