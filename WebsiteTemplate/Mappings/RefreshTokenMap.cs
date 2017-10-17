using BasicAuthentication.Security;
using FluentNHibernate.Mapping;
using WebsiteTemplate.Data;

namespace WebsiteTemplate.Mappings
{
    public class RefreshTokenMap : ClassMap<RefreshToken>
    {
        public RefreshTokenMap()
        {
            Table("RefreshToken");

            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.ClientId).Not.Nullable().Length(50);
            Map(x => x.ExpiresUtc).Not.Nullable();
            Map(x => x.IssuedUtc).Not.Nullable();
            if (DataStore.ProviderName.Contains("MySql"))
            {
                Map(x => x.ProtectedTicket).Not.Nullable().CustomType("StringClob").CustomSqlType("LONGTEXT").Length(int.MaxValue);
            }
            else if (DataStore.SetCustomSqlTypes == true)
            {
                Map(x => x.ProtectedTicket).Not.Nullable().CustomType("StringClob").CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            }
            else
            {
                Map(x => x.ProtectedTicket).Not.Nullable().Length(int.MaxValue);
            }
            Map(x => x.Subject).Not.Nullable().Length(50);
        }
    }
}