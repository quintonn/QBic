using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Mappings;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Data
{
    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            Trace.WriteLine("??" + sql.ToString() + "??");
            //Console.WriteLine(sql.ToString());
            return sql;
        }
    }

    public static class NothingJere
    {
        public static FluentMappingsContainer AddFromAssemblyOf2<T>(this FluentMappingsContainer mappings)
        {
            var tempQ = new DynamicMap<DynamicClass>();
            
            var container = mappings.AddFromAssemblyOf<User>();

            foreach (var type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(DynamicClass))))
            {
                var d1 = typeof(DynamicMap<>);
                Type[] typeArgs = { type };
                var makeme = d1.MakeGenericType(typeArgs);
                container.Add(makeme);
            }

            return container;
        }
    }

    public class DataStore
    {
        private static object xlock = new object();

        private static ISessionFactory Store;
        private static NHibernate.Cfg.Configuration Configuration;

        static DataStore()
        {
            Store = CreateSessionFactory();

            new SchemaUpdate(Configuration).Execute(true, true);
        }

        private static ISessionFactory CreateSessionFactory()
        {
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
              
              .Mappings(m => m.FluentMappings.AddFromAssemblyOf2<User>());
            
            config.ExposeConfiguration(x =>
            {
                x.SetInterceptor(new SqlStatementInterceptor());
                x.Properties.Add("use_proxy_validator", "false");
            });
            Configuration = config.BuildConfiguration();

            return Configuration.BuildSessionFactory();
        }

        public DataStore()
        {


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

        public void Save<T>(T item) where T : BaseClass
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