using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.TestItems;
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

        private DataStore(ApplicationSettingsCore appSettings)
        {
            AppSettings = appSettings;
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

        //static DataStore()
        private void init()
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            
            Store = CreateSessionFactory();

            new SchemaUpdate(Configuration).Execute(true, true);
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

            Configuration = CreateNewConfigurationUsingConnectionString(mainConnectionString);
            
            return Configuration.BuildSessionFactory();
        }

        public NHibernate.Cfg.Configuration CreateNewConfigurationUsingConnectionString(string connectionString)
        {
            IPersistenceConfigurer configurer;
            DataStore.SetCustomSqlTypes = true;
            if (connectionString.Contains("##CurrentDirectory##"))
            {
                var currentDirectory = HttpRuntime.AppDomainAppPath;
                connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

                configurer = FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard.ConnectionString(connectionString);
                DataStore.SetCustomSqlTypes = false;
            }
            else
            {
                configurer = FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012.ConnectionString(connectionString);
            }

            var config = Fluently.Configure()
              .Database(configurer)
              
              .Mappings(m => m.FluentMappings.CustomAddFromAssemblyOf<User>().Conventions.Add<JoinedSubclassIdConvention>());

            config.ExposeConfiguration(x =>
            {
                //x.SetInterceptor(new SqlStatementInterceptor());
                x.Properties.Add("use_proxy_validator", "false"); // to ignore public/internal fields on model classes
            });
            var configuration = config.BuildConfiguration();

            return configuration;

            //return configuration.BuildSessionFactory();
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
                    else
                    {
                        Connection = new System.Data.SqlClient.SqlConnection(connectionString);
                    }
                    Connection.Open();
                }
            }
            return Store.OpenSession(Connection);
        }
    }
}