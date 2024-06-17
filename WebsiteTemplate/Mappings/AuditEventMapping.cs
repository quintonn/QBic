using NHibernate.Type;
using QBic.Core.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class AuditEventMapping : BaseClassMap<AuditEvent>
    {
        public AuditEventMapping()
        {
            Table("AuditEvent");

            Map(x => x.UserId).Not.Nullable();
            Map(x => x.UserName).Not.Nullable();

            Map(x => x.AuditEventDateTimeUTC).Not.Nullable();

            Map(x => x.AuditAction).CustomType<EnumStringType<AuditAction>>()
                                   .Not
                                   .Nullable();
            Map(x => x.ObjectId).Not.Nullable();
            Map(x => x.EntityName).Not.Nullable();

            if (DataStore.DbProviderType == DBProviderType.MYSQL)
            {
                Map(x => x.OriginalObject).Nullable().CustomType("StringClob").CustomSqlType("LONGTEXT").Length(int.MaxValue);
                Map(x => x.NewObject).Nullable().CustomType("StringClob").CustomSqlType("LONGTEXT").Length(int.MaxValue);
            }
            else if (DataStore.DbProviderType == DBProviderType.MSSQL)
            {
                Map(x => x.OriginalObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
                Map(x => x.NewObject).Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            }
            else
            {
                Map(x => x.OriginalObject).Nullable().Length(int.MaxValue);
                Map(x => x.NewObject).Nullable().Length(int.MaxValue);
            }
        }
    }
}