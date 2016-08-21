using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Configuration;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Data
{
    public class DataStore
    {
         need to prevent global access to data store for save/update/delete. 
         this needs to go through data service so auditing etc can be done.
         maybe also for retrievals, not sure if that should be audited too. - maybe make this configurable. could impact performance
         to prevent performace impact, this might have to be handled by a background thread or something like that.
        private static DataStore _instance { get; set; }

        private DataStore()
        {
        }

        public static DataStore GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DataStore();
            }
            return _instance;
        }

        private static object xlock = new object();

        private static ISessionFactory Store;
        private static NHibernate.Cfg.Configuration Configuration;

        static DataStore()
        {
            //if (Debugger.IsAttached == false) Debugger.Launch();
            
            Store = CreateSessionFactory();

            new SchemaUpdate(Configuration).Execute(true, true);
        }

        private static ISessionFactory CreateSessionFactory()
        {
            //if (Debugger.IsAttached == false) Debugger.Launch();
            var container = new FluentMappingsContainer();
            var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
            if (String.IsNullOrWhiteSpace(mainConnectionString))
            {
                throw new ArgumentNullException("MainDataStore connection string property in web.config does not contain a value for connection string");
            }

            var config = Fluently.Configure()
              .Database(

                FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012.ConnectionString(mainConnectionString)
              )

              .Mappings(m => m.FluentMappings.CustomAddFromAssemblyOf<User>().Conventions.Add<JoinedSubclassIdConvention>());
            
            config.ExposeConfiguration(x =>
            {
                //x.SetInterceptor(new SqlStatementInterceptor());
                x.Properties.Add("use_proxy_validator", "false");
            });
            Configuration = config.BuildConfiguration();

            return Configuration.BuildSessionFactory();
        }

        public void ResetData()
        {
            //new SchemaExport(Configuration).Execute(false, true, false);
            new SchemaUpdate(Configuration).Execute(true, true);
        }

        public ISession OpenSession()
        {
            return Store.OpenSession();
        }
    }
}