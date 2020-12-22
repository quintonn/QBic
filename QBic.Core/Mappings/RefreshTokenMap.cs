using FluentNHibernate.Mapping;
using QBic.Core.Data;
using QBic.Core.Models;

namespace QBic.Core.Mappings
{
    public class RefreshTokenMap : ClassMap<RefreshToken>
    {
        public RefreshTokenMap()
        {
            Table("RefreshToken");

            Id(x => x.Id).GeneratedBy.Assigned();

            if (DataStore.ProviderName == "MYSQL")
            {
                Map(x => x.Token).Not.Nullable().CustomType("StringClob").CustomSqlType("LONGTEXT").Length(int.MaxValue);
            }
            else if(DataStore.ProviderName == "SQL")
            {
                Map(x => x.Token).Not.Nullable().CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            }
            else
            {
                Map(x => x.Token).Not.Nullable().Length(int.MaxValue);
            }
        }
    }
}