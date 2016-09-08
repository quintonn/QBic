using FluentNHibernate.Mapping;
using System.Collections.Generic;
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
            Map(x => x.ObjectId).Not.Nullable();
            Map(x => x.EntityName).Not.Nullable();
            Map(x => x.OriginalObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            Map(x => x.NewObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);

            //HasMany<EventRoleAssociation>(FluentNHibernate.Reveal.Member<AuditEvent, IEnumerable<EventRoleAssociation>>("EventRoles"));

            //HasMany<EventRoleAssociation>(x => x.EventRoles).KeyColumn("IdAuditEvent");
        }
    }
}