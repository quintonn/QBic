using BasicAuthentication.Security;
using FluentNHibernate.Mapping;

namespace WebsiteTemplate.Mappings
{
    public class RefreshTokenMap : ClassMap<RefreshToken>
    {
        public RefreshTokenMap()
        {
            Table("RefreshTokens");

            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.ClientId).Not.Nullable().Length(50);
            Map(x => x.ExpiresUtc).Not.Nullable();
            Map(x => x.IssuedUtc).Not.Nullable();
            Map(x => x.ProtectedTicket).Not.Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            Map(x => x.Subject).Not.Nullable().Length(50);
        }
    }
}