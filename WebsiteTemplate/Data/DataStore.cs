using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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

    public class JoinedSubclassIdConvention : IJoinedSubclassConvention, IJoinedSubclassConventionAcceptance
    {
        public void Apply(IJoinedSubclassInstance instance)
        {
            instance.Key.Column("Id");
        }

        public void Accept(IAcceptanceCriteria<IJoinedSubclassInspector> criteria)
        {
            criteria.Expect(x => true);
        }
    }

    public static class DataStoreExtensionMethods
    {
        public static FluentMappingsContainer CustomAddFromAssemblyOf<T>(this FluentMappingsContainer mappings)
        {
            var tempQ = new DynamicMap<DynamicClass>();
            
            var container = mappings.AddFromAssemblyOf<User>();

            var curDir = System.Web.HttpRuntime.AppDomainAppPath;
            var dlls = Directory.GetFiles(curDir, "*.dll", SearchOption.AllDirectories);
            var types = new List<Type>();

            var appDomain = AppDomain.CreateDomain("tmpDomainForWebTemplate");
            foreach (var dll in dlls)
            {
                if (dll.Contains("\\roslyn\\"))
                {
                    continue;
                }

                try
                {
                    var assembly = appDomain.Load(File.ReadAllBytes(dll));
                    var dynamicTypes = assembly.GetTypes().Where(myType => myType.IsClass /*&& !myType.IsAbstract */&& myType.IsSubclassOf(typeof(DynamicClass)));
                    types.AddRange(dynamicTypes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            var list = new List<string>();
            foreach (var type in types)
            {
                var typeString = type.ToString();
                if (list.Contains(typeString))
                {
                    continue;
                }
                list.Add(typeString);
                if (type.BaseType == typeof(DynamicClass))
                {
                    var d1 = typeof(DynamicMap<>);
                    Type[] typeArgs = { type };
                    var makeme = d1.MakeGenericType(typeArgs);
                    container.Add(makeme);
                }
                else
                {
                    var d1 = typeof(ChildDynamicMap<>);
                    Type[] typeArgs = { type };
                    var makeme = d1.MakeGenericType(typeArgs);
                    container.Add(makeme);
                }
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