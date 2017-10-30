using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Web;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Data
{
    public class DataStore
    {
        // need to prevent global access to data store for save/update/delete. 
        // this needs to go through data service so auditing etc can be done.
        // maybe also for retrievals, not sure if that should be audited too. - maybe make this configurable. could impact performance
        // to prevent performace impact, this might have to be handled by a background thread or something like that.

        //for now just use nhibernate                                                                            

        //-- Long term maybe make my own query language??
        private static DataStore _instance { get; set; }

        private ApplicationSettingsCore AppSettings { get; set; }

        internal static bool SetCustomSqlTypes { get; set; }

        private static bool UpdateDatabase { get; set; }
        internal static string ProviderName { get; set; }

        private DataStore(ApplicationSettingsCore appSettings)
        {
            AppSettings = appSettings;
            UpdateDatabase = appSettings.UpdateDatabase;
            init();
        }

        internal static DataStore GetInstance(ApplicationSettingsCore appSettings)
        {
            if (_instance == null)
            {
                _instance = new DataStore(appSettings);
            }
            return _instance;
        }

        private static ISessionFactory Store;
        private static NHibernate.Cfg.Configuration Configuration;

        public string GetTableName(Type type)
        {
            var tableName = Configuration.GetClassMapping(type).Table.Name;// .GetClassMetadata(typeof(T)) as NHibernate.Persister.Entity.AbstractEntityPersister;
            return tableName;
            //string table = metadata.TableName;

            //using (ISession session = sessionFactory.OpenSession())
            //{
            //    using (var transaction = session.BeginTransaction())
            //    {
            //        string deleteAll = string.Format("DELETE FROM \"{0}\"", table);
            //        session.CreateSQLQuery(deleteAll).ExecuteUpdate();

            //        transaction.Commit();
            //    }
            //}
        }

        //static DataStore()
        private void init()
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            
            Store = CreateSessionFactory();

            if (UpdateDatabase)
            {
                new SchemaUpdate(Configuration).Execute(false, UpdateDatabase);
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            //if (Debugger.IsAttached == false) Debugger.Launch();
            var container = new FluentMappingsContainer();

            var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
            //mainConnectionString = Encryption.Encrypt(mainConnectionString, AppSettings.ApplicationPassPhrase);

            //mainConnectionString = Encryption.Decrypt(mainConnectionString, AppSettings.ApplicationPassPhrase);

            if (String.IsNullOrWhiteSpace(mainConnectionString))
            {
                throw new ArgumentNullException("MainDataStore connection string property in web.config does not contain a value for connection string");
            }

            Configuration = CreateNewConfigurationUsingConnectionString(mainConnectionString, ConfigurationManager.ConnectionStrings["MainDataStore"]?.ProviderName);
            
            return Configuration.BuildSessionFactory();
        }

        public NHibernate.Cfg.Configuration CreateNewConfigurationUsingConnectionString(string connectionString, string providerName)
        {
            IPersistenceConfigurer configurer;
            DataStore.SetCustomSqlTypes = true;
            DataStore.ProviderName = providerName;

            if (connectionString.Contains("##CurrentDirectory##"))
            {
                var currentDirectory = HttpRuntime.AppDomainAppPath;
                connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

                configurer = FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
                DataStore.SetCustomSqlTypes = false;
            }
            else if (providerName.Contains("MySql"))
            {
                configurer = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            }
            else
            {
                configurer = FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            }


            var config = Fluently.Configure()
              .Database(configurer)

              .Mappings(m => m.FluentMappings.CustomAddFromAssemblyOf<User>(AppSettings).Conventions.Add<JoinedSubclassIdConvention>());

            config.ExposeConfiguration(x =>
            {
                //x.SetInterceptor(new SqlStatementInterceptor());
                x.Properties.Add("use_proxy_validator", "false"); // to ignore public/internal fields on model classes
                //x.DataBaseIntegration(prop =>
                //{
                //    prop.BatchSize = 50;
                //    prop.Batcher<NHibernate.AdoNet.MySqlClientBatchingBatcherFactory>();
                //});
            });

            var configuration = config.BuildConfiguration();

            return configuration;
        }

        public void ResetData()
        {
            //new SchemaExport(Configuration).Execute(false, true, false);
            new SchemaUpdate(Configuration).Execute(true, true);
        }

        static IDbConnection Connection { get; set; }
        static object locker = new object();

        public static void KillConnection()
        {
            Connection.Close();
            Connection = null;
        }
        public ISession OpenSession()
        {
            lock (locker)
            {
                if (Connection == null)
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
                    if (connectionString.Contains("##CurrentDirectory##"))
                    {
                        var currentDirectory = HttpRuntime.AppDomainAppPath;
                        connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory);
                        Connection = new SQLiteConnection(connectionString);
                    }
                    else if (ConfigurationManager.ConnectionStrings["MainDataStore"]?.ProviderName.Contains("MySql") == true)
                    {
                        Connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
                    }
                    else
                    {
                        Connection = new System.Data.SqlClient.SqlConnection(connectionString);
                    }
                    Connection.Open();
                }
            }
            return Store.OpenSession(Connection);
        }

        public void CloseSession()
        {
            KillConnection();
        }
    }
}