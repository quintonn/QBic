using Benoni.Core.Data;
using FluentNHibernate.Mapping;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class AuditEventMapping : BaseClassMap<AuditEvent>
    {
        public AuditEventMapping()
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            Table("AuditEvent");

            References(x => x.User).Column("IdUser")
                           //.Not.Nullable()
                           //.Cascade.Delete()
                           .Nullable()
                           .NotFound.Ignore()
                           .LazyLoad(Laziness.False);

            Map(x => x.AuditEventDateTimeUTC).Not.Nullable();

            Map(x => x.AuditAction).CustomType<GenericEnumMapper<AuditAction>>()
                                   .Not
                                   .Nullable();
            Map(x => x.ObjectId).Not.Nullable();
            Map(x => x.EntityName).Not.Nullable();
            //if (DataStore.ProviderName.Contains("MySql"))
            //{
            //    Map(x => x.OriginalObject).Nullable().CustomSqlType("LONGTEXT").Length(int.MaxValue);
            //    Map(x => x.NewObject).Nullable().CustomSqlType("LONGTEXT").Length(int.MaxValue);
            //}
            if (DataStore.SetCustomSqlTypes == true)
            {
                Map(x => x.OriginalObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
                Map(x => x.NewObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            }
            else if (DataStore.SetCustomSqlTypes == false)
            {
                Map(x => x.OriginalObject).Nullable().Length(int.MaxValue);
                Map(x => x.NewObject).Nullable().Length(int.MaxValue);
            }
        }
    }
}