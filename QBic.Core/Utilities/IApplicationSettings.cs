using FluentNHibernate.Cfg.Db;
using QBic.Core.Data;

namespace QBic.Core.Utilities
{
    public interface IApplicationSettings
    {
        IPersistenceConfigurer GetPersistenceConfigurer(string databaseName);
        DBProviderType DataProviderType { get; }
        bool ShowSQL { get; }
        GoogleBackupConfig GoogleBackupConfig { get; }
    }
}