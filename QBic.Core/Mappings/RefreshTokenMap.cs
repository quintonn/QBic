using FluentNHibernate.Mapping;
using QBic.Core.Models;
using System;

namespace QBic.Core.Mappings
{
    public class RefreshTokenMap : ClassMap<RefreshToken>
    {
        public RefreshTokenMap()
        {
            Console.WriteLine("Inside RefreshTokenMap");
            Table("RefreshToken");

            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.Token).Not.Nullable().Length(int.MaxValue);//.CustomType("StringClob").CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            Console.WriteLine("End of refresh token map");
            //Map(x => x.ClientId).Not.Nullable().Length(50);
            //Map(x => x.ExpiresUtc).Not.Nullable();
            //Map(x => x.IssuedUtc).Not.Nullable();
            ////if (DataStore.ProviderName.Contains("MySql"))
            ////{
            ////    Map(x => x.ProtectedTicket).Not.Nullable().CustomType("StringClob").CustomSqlType("LONGTEXT").Length(int.MaxValue);
            ////}
            //if (DataStore.SetCustomSqlTypes == true)
            //{
            //    Map(x => x.ProtectedTicket).Not.Nullable().CustomType("StringClob").CustomSqlType("nvarchar(max)").Length(int.MaxValue);
            //}
            //else
            //{
            //    Map(x => x.ProtectedTicket).Not.Nullable().Length(int.MaxValue);
            //}
            //Map(x => x.Subject).Not.Nullable().Length(50);
        }
    }
}