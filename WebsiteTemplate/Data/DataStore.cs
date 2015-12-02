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
            //Configuration = new Configuration();
            ////Configuration.Configure(typeof(DataStore).Assembly, "JBQ.Data.Mappings.hibernate.cfg.xml");
            //Configuration.AddAssembly(typeof(User).Assembly);
            //Store = Configuration.BuildSessionFactory();

            Store = CreateSessionFactory();

            new SchemaUpdate(Configuration).Execute(true, true);
            
            //new SchemaExport(Configuration).Execute(false, true, false);//, false);
        }

        private static ISessionFactory CreateSessionFactory()
        {
            var container = new FluentMappingsContainer();
            var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"].ConnectionString;

            var config = Fluently.Configure()
              .Database(

                FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012.ConnectionString(mainConnectionString)
                //.UsingFile("firstProject.db")
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

        public static string GetStorageDrive()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "\\SnappData";
            return path;
        }

        public List<User> GetUsers(string callingUserId)
        {
            using (var session = Store.OpenSession())
            {
                var user = session.Load<User>(callingUserId);
                if (user == null || String.IsNullOrWhiteSpace(user.Id))
                {
                    throw new Exception("No user found with id " + callingUserId);
                }

                var result = session.CreateCriteria<User>()
                                        .CreateAlias("UserRole", "role")
                                        .Add(Restrictions.Eq("EmailConfirmed", true))
                                        //.Add(Restrictions.Ge("role.Id", user.UserRole.Id))
                                        .List<User>()
                                        .ToList();
                return result;
            }
        }

        public void Save<T>(T item) where T : BaseClass
        {
            System.Diagnostics.Trace.TraceInformation("saving an object : " + item.GetType().ToString());
            using (var session = Store.OpenSession())
            {
                //var id = item.Id;
                //if (id != null && !String.IsNullOrWhiteSpace(id))
                //{
                //    /// I don't know why i'm doing this
                //    var existingItem = session.CreateCriteria<T>()
                //                              .Add(Restrictions.Eq("Id", id))
                //                              .UniqueResult<T>();
                //    if (existingItem != null && id != null && !String.IsNullOrWhiteSpace(id.ToString()))
                //    {
                //        throw new Exception("Item with id " + id + " already exists.\n" + JsonConvert.SerializeObject(existingItem));
                //    }
                //}
                session.SaveOrUpdate(item);
                //session.Save(item);
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