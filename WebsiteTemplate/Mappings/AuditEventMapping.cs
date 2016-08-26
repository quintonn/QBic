using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class AuditEventMapping : BaseClassMap<AuditEvent>
    {
        public AuditEventMapping()
        {
            Table("AuditEvents");

            References(x => x.User).Column("IdUser")
                           .Not.Nullable()
                           .LazyLoad(Laziness.False);

            Map(x => x.AuditEventDateTimeUTC).Not.Nullable();

            Map(x => x.AuditAction).CustomType<GenericEnumMapper<AuditAction>>()
                                   .Not
                                   .Nullable();
            Map(x => x.ObjectId).Nullable();
            Map(x => x.OriginalObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            Map(x => x.NewObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
        }
    }
}