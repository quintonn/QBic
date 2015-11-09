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
        public virtual string testColumn { get; set; }

        public virtual string Id { get; set; }

        public virtual bool CanDelete { get; protected set; }
    }

    public class TestChildClass : TestClass
    {
        public virtual string Namex { get; set; }
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

            foreach (var column in columnsToAdd)
            {
                if (column == "CanDelete" || column == "Id" || column == "Items")
                {
                    continue;
                }
                Map(FluentNHibernate.Reveal.Member<TestChildClass>(column));
            }
        }
    }
}