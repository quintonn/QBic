using FluentNHibernate.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
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
        }

        public virtual string Id { get; set; }

        public virtual bool CanDelete { get; protected set; }

        public virtual Dictionary<string, string> Items { get; set; }
    }


    public class TestDynamicMap : ClassMap<TestClass>
    {
        public TestDynamicMap()
        {
            Table("TestTable");

            Id(x => x.Id).GeneratedBy.Custom<CustomIdentifierGenerator>();

            Map(x => x.CanDelete).Default("1")
                                 .Not.Nullable();

            DynamicComponent(x => x.Items, dc =>
                {
                    dc.Map<string>("testColumn");
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
                });
        }
    }
}