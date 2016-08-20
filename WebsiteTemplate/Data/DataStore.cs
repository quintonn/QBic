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
         I think this class is doing too much. and it's not being resolved using unity container.
         maybe i can call the register before configure in the startup class
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

        public void SaveOrUpdate<T>(T item) where T : BaseClass
        {
            System.Diagnostics.Trace.TraceInformation("saving an object : " + item.GetType().ToString());
            using (var session = Store.OpenSession())
            {
                session.SaveOrUpdate(item);
                session.Flush();
            }
            System.Diagnostics.Trace.TraceInformation("object saved: " + item.GetType().ToString());
        }

        public bool TryDelete(BaseClass item)
        {
            if (item.CanDelete == false)
            {
                return false;
            }
            using (var session = Store.OpenSession())
            {
                session.Delete(item);
                session.Flush();
            }
            return true;
        }
    }
}