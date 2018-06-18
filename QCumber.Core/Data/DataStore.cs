using QCumber.Core.Utilities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace QCumber.Core.Data
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

        public static bool SetCustomSqlTypes { get; set; }

        private static bool UpdateDatabase { get; set; }
        
        private DataStore(bool updateDatabase)
        {
            UpdateDatabase = updateDatabase;
            init();
        }

        public static DataStore GetInstance(bool updateDatabase)
        {
            if (_instance == null)
            {
                _instance = new DataStore(updateDatabase);
            }
            return _instance;
        }

        private static ISessionFactory Store;
        private static NHibernate.Cfg.Configuration Configuration;

        //public string GetTableName(Type type)
        //{
        //    var tableName = Configuration.GetClassMapping(type).Table.Name;
        //    return tableName;
        //}

        private void init()
        {
            Store = CreateSessionFactory();

            if (UpdateDatabase)
            {
                new SchemaUpdate(Configuration).Execute(false, UpdateDatabase);
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            var container = new FluentMappingsContainer();

            var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
            //mainConnectionString = Encryption.Encrypt(mainConnectionString, AppSettings.ApplicationPassPhrase);

            //mainConnectionString = Encryption.Decrypt(mainConnectionString, AppSettings.ApplicationPassPhrase);

            if (String.IsNullOrWhiteSpace(mainConnectionString))
            {
                throw new ArgumentNullException("MainDataStore connection string property in web.config does not contain a value for connection string");
            }

            Configuration = CreateNewConfigurationUsingConnectionString(mainConnectionString);

            try
            {
                var result = Configuration.BuildSessionFactory();
                return result;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                throw;
            }
        }

        public NHibernate.Cfg.Configuration CreateNewConfigurationUsingConnectionString(string connectionString)
        {
            IPersistenceConfigurer configurer;
            SetCustomSqlTypes = true;
            
            if (connectionString.Contains("##CurrentDirectory##") || connectionString.Contains(":memory:"))
            {
                var currentDirectory = QCumberUtils.GetCurrentDirectory();
                connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

                configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
                DataStore.SetCustomSqlTypes = false;
            }
            //else if (providerName.Contains("MySql"))
            //{
            //    configurer = MySQLConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            //}
            else
            {
                configurer = MsSqlConfiguration.MsSql2012.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            }


            var config = Fluently.Configure()
              .Database(configurer)

              .Mappings(m => m.FluentMappings.AddFromRunningAssemblies().Conventions.Add<JoinedSubclassIdConvention>());

            config.ExposeConfiguration(x =>
            {
                //if (AppSettings.ShowSQL == true)
                //{
                //    x.SetInterceptor(new SqlStatementInterceptor());
                //}

                // This will set the command_timeout property on factory-level
                //x.SetProperty(NHibernate.Cfg.Environment.CommandTimeout, "180");

                // This will set the command_timeout property on system-level
                //NHibernate.Cfg.Environment.Properties.Add(NHibernate.Cfg.Environment.CommandTimeout, "180");

                x.Properties.Add("use_proxy_validator", "false"); // to ignore public/internal fields on model classes
                //x.DataBaseIntegration(prop =>
                //{
                //    prop.BatchSize = 50;
                    //prop.Batcher<NHibernate.AdoNet.MySqlClientBatchingBatcherFactory>();
                //});
            });

            var configuration = config.BuildConfiguration();

            return configuration;
        }

        //public void ResetData()
        //{
        //    //new SchemaExport(Configuration).Execute(false, true, false);
        //    new SchemaUpdate(Configuration).Execute(true, true);
        //}

        public ISession OpenSession()
        {
            return Store.OpenSession();
        }

        public IStatelessSession OpenStatelessSession()
        {
            return Store.OpenStatelessSession();
        }

        public DbConnection CreateConnection(string connectionString)
        {
            if (connectionString.Contains("##CurrentDirectory##") || connectionString.Contains(":memory:"))
            {
                connectionString = connectionString.Replace("##CurrentDirectory##", QCumberUtils.GetCurrentDirectory());
                return new SQLiteConnection(connectionString);
            }
            else if (ConfigurationManager.ConnectionStrings["MainDataStore"]?.ProviderName.Contains("MySql") == true)
            {
                throw new NotImplementedException("Code for MySql has not been enabled due to many problems with concurrent database access using MySql");
                //return new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            }
            else
            {
                return new System.Data.SqlClient.SqlConnection(connectionString);
            }
        }
    }
}