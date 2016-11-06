﻿using BasicAuthentication.Security;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using WebsiteTemplate.Backend.TestItems;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
using WebsiteTemplate.Mappings;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;
//using Microsoft.SqlServer.Smo;

namespace WebsiteTemplate.Backend.Services
{
    public class BasicClassEqualityComparer : IEqualityComparer<BaseClass>
    {
        public bool Equals(BaseClass x, BaseClass y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(BaseClass obj)
        {
            return 0;
        }
    }

    public class BackupService
    {
        private static Dictionary<int, Type> SystemTypes { get; set; }

        private DataService DataService { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }

        public static readonly string BACKUP_HEADER_KEY = "BackupType";

        public BackupService(DataService dataService, ApplicationSettingsCore appSettings)
        {
            DataService = dataService;
            AppSettings = appSettings;

            if (SystemTypes == null)
            {
                SystemTypes = new Dictionary<int, Type>();
                SystemTypes.Add(1, typeof(AuditEvent));
                SystemTypes.Add(2, typeof(UserRoleAssociation));
                SystemTypes.Add(3, typeof(EventRoleAssociation));
                SystemTypes.Add(4, typeof(Menu));
                SystemTypes.Add(5, typeof(UserRole));
                SystemTypes.Add(6, typeof(User));
                SystemTypes.Add(7, typeof(BackgroundJobResult));
                SystemTypes.Add(8, typeof(CauseChild));
                SystemTypes.Add(9, typeof(SuperCause));
                SystemTypes.Add(10, typeof(Models.SystemSettings));

                AppSettings.ConfigureSiteSpecificTypes(SystemTypes);
            }
        }

        public byte[] CreateBackupOfAllData()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
            if (connectionString.Contains("##CurrentDirectory##"))
            {
                var currentDirectory = HttpRuntime.AppDomainAppPath;
                var filePath = currentDirectory + "\\Data\\appData.db";
                DataStore.KillConnection();

                var data = File.ReadAllBytes(filePath);

                var bytes = CompressionHelper.DeflateByte(data, Ionic.Zlib.CompressionLevel.BestCompression);
                return bytes;
            }

            //Microsoft.SqlServer.Management.Smo;

            using (var session = DataService.OpenSession())
            {
                byte[] bytes;
                using (var output = new MemoryStream())
                {
                    using (var compressor = new Ionic.Zlib.DeflateStream(output, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestCompression))
                    {
                        var ids = SystemTypes.Keys.ToList().OrderBy(i => i);
                        foreach (var id in ids)
                        {
                            var type = SystemTypes[id];

                            var createCriteriaMethodInfo = typeof(ISession).GetMethods().FirstOrDefault(m => m.Name == "CreateCriteria"
                                                            && m.GetParameters().Count() == 0);
                            var createCriteriaMethod = createCriteriaMethodInfo.MakeGenericMethod(type);
                            var count = (createCriteriaMethod.Invoke(session, null) as ICriteria)
                                        .SetProjection(Projections.Count(Projections.Id()))
                                        .List<int>()
                                        .Sum();

                            var added = 0;

                            while (added != count)
                            {
                                var nextLoad = 100;

                                var queryOverMethodInfo = typeof(ISession).GetMethods().FirstOrDefault(m => m.Name == "QueryOver"
                                                                && m.GetParameters().Count() == 0);
                                var queryOverMethod = queryOverMethodInfo.MakeGenericMethod(type);

                                var genericType = typeof(IQueryOver<>);
                                Type[] typeArgs = { type };

                                var queryOver = queryOverMethod.Invoke(session, null);

                                var gType = genericType.MakeGenericType(typeArgs);

                                var skip = gType.GetMethod("Skip");
                                queryOver = skip.Invoke(queryOver, new object[] { added });

                                var take = gType.GetMethod("Take");
                                queryOver = take.Invoke(queryOver, new object[] { nextLoad });

                                var list = gType.GetMethods().FirstOrDefault(m => m.Name == "List"
                                                                && m.GetParameters().Count() == 0);

                                var enumerableItems = list.Invoke(queryOver, null) as IEnumerable;
                                var items = enumerableItems.OfType<BaseClass>().ToList();

                                var json = JsonHelper.SerializeObject(items, false, true);
                                var data = XXXUtils.GetBytes(json);
                                compressor.Write(data, 0, data.Length);

                                added += items.Count;
                            }
                        }
                    }

                    bytes = output.ToArray();
                }

                return bytes;
            }
        }

        public void CreateNewDatabaseSchema(string connectionString)
        {
            var store = DataStore.GetInstance(null);

            DynamicClass.SetIdsToBeAssigned = true; // This will set the Fluent NHibernate mappings' id's to be assigned and not GUID's for the restore.

            //var connectionString = "Integrated Security=SSPI;Persist Security Info=False;Data Source=localhost;Initial Catalog=WebsiteTemplate";
            var config = store.CreateNewConfigurationUsingConnectionString(connectionString);
            new SchemaUpdate(config).Execute(true, true);

            //var factory = config.BuildSessionFactory();

            DynamicClass.SetIdsToBeAssigned = false; // Change it back
        }

        public void RemoveExistingData(string connectionString)
        {
            var store = DataStore.GetInstance(null);
            var config = store.CreateNewConfigurationUsingConnectionString(connectionString);
            var factory = config.BuildSessionFactory();

            IList<BaseClass> items = null;
            using (var session = factory.OpenSession())
            {
                items = session.QueryOver<BaseClass>().List<BaseClass>();
            }

            //Delete(dbItems, factory);

            var comparer = new BasicClassEqualityComparer();

            var users = items.Where(i => i is User).ToList();
            var userRoles = items.Where(i => i is UserRole).ToList();
            var eventRoleItems = items.Where(i => i is EventRoleAssociation).ToList();
            var auditEvents = items.Where(i => i is AuditEvent).ToList();
            var otherItems = items.Except(users, comparer)
                                  .Except(userRoles, comparer)
                                  .Except(eventRoleItems, comparer)
                                  .Except(auditEvents, comparer)
                                  .ToList();

            Delete(otherItems, factory);
            Delete(eventRoleItems, factory);
            Delete(auditEvents, factory);
            Delete(userRoles, factory);
            Delete(users, factory);

            using (var session = factory.OpenSession())
            {
                var count = session
                        .CreateCriteria<BaseClass>()
                        .SetProjection(
                            Projections.Count(Projections.Id())
                        )
                        .List<int>()
                        .Sum();
                if (count != 0)
                {
                    throw new Exception("Not all items were deleted. Contact support");
                }
            }
        }

        private void Delete(IList<BaseClass> items, ISessionFactory factory, int count = 10)
        {
            var itemCount = Math.Max(100, items.Count / 2);
            var itemsToDelete = items;
            var nextItems = itemsToDelete.Take(itemCount).ToList();
            var cnt = 0;
            var errorCnt = 0;
            while (itemsToDelete.Count > 0)
            {
                try
                {
                    if (nextItems.Count > 0)
                    {
                        DeleteItems(nextItems, factory);
                        using (var session = factory.OpenSession())
                        {
                            var dbItems = session.QueryOver<BaseClass>().Where(Restrictions.On<UserRole>(x => x.Id).IsIn(items.Select(i => i.Id).ToArray())).List<BaseClass>();
                            itemsToDelete = dbItems.ToList();
                        }
                    }
                    else
                    {
                        cnt++;
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.InnerException?.Message);
                    errorCnt++;
                    cnt++;

                    if (itemCount >= itemsToDelete.Count)
                    {
                        itemCount = itemsToDelete.Count / 2;
                        itemCount = Math.Max(itemCount, 1);
                    }

                    if (errorCnt == 10)
                    {
                        itemCount--;
                        errorCnt = 0;
                        cnt = 0;
                    }
                    if (itemCount < 0)
                    {
                        throw new Exception("Unable to complete delete of existing items");
                    }
                }

                if ((itemCount * cnt) > itemsToDelete.Count)
                {
                    cnt = 0;
                }
                nextItems = itemsToDelete.Skip(itemCount * cnt).Take(itemCount).ToList();
            }
        }

        private void DeleteItems(IList<BaseClass> items, ISessionFactory factory, int count = 10)
        {
            var failedItems = new List<BaseClass>();
            using (var session = factory.OpenStatelessSession())
            {
                session.SetBatchSize(items.Count);
                foreach (var item in items)
                {
                    session.Delete(item);
                }
            }
        }

        public void RestoreBackupOfAllData(byte[] data, string connectionString)
        {
            var tmpBytes = CompressionHelper.InflateByte(data);
            var tmpString = XXXUtils.GetString(tmpBytes);

            var items = JsonHelper.DeserializeObject<List<BaseClass>>(tmpString, true);

            //CreateNewDatabaseSchema(connectionString); //Not sure if this should be an explicit call or not.

            try
            {
                DynamicClass.SetIdsToBeAssigned = true;
                var store = DataStore.GetInstance(null);
                //var connectionString = "Integrated Security=SSPI;Persist Security Info=False;Data Source=localhost;Initial Catalog=WebsiteTemplate";
                var config = store.CreateNewConfigurationUsingConnectionString(connectionString);
                var factory = config.BuildSessionFactory();

                var comparer = new BasicClassEqualityComparer();

                var users = items.Where(i => i is User).ToList();
                var eventRoleItems = items.Where(i => i is EventRoleAssociation).ToList();
                var auditEvents = items.Where(i => i is AuditEvent).ToList();
                var otherItems = items.Except(users, comparer).Except(eventRoleItems, comparer).Except(auditEvents, comparer).ToList();

                Insert(users, factory);
                Insert(otherItems, factory);
                Insert(eventRoleItems, factory);
                Insert(auditEvents, factory);

                using (var session = factory.OpenSession())
                {
                    var count = session
                            .CreateCriteria<BaseClass>()
                            .SetProjection(
                                Projections.Count(Projections.Id())
                            )
                            .List<int>()
                            .Sum();
                    if (count != items.Count)
                    {
                        throw new Exception("Not all items were restored. Contact support");
                    }
                }
            }
            finally
            {
                DynamicClass.SetIdsToBeAssigned = false; // Change it back
            }
        }

        private void Insert(IList<BaseClass> items, ISessionFactory factory, int count = 10)
        {
            var itemCount = Math.Max(100, items.Count / 2);
            var itemsToAdd = items;
            var nextItems = itemsToAdd.Take(itemCount).ToList();
            var cnt = 0;
            var errorCnt = 0;

            var comparer = new BasicClassEqualityComparer();
            while (itemsToAdd.Count > 0)
            {
                try
                {
                    if (nextItems.Count > 0)
                    {
                        InsertItems(nextItems, factory);
                        using (var session = factory.OpenSession())
                        {
                            //var dbItems = session.QueryOver<BaseClass>().List<BaseClass>();
                            var dbItems = session.QueryOver<BaseClass>().Where(Restrictions.On<UserRole>(x => x.Id).IsIn(items.Select(i => i.Id).ToArray())).List<BaseClass>();

                            itemsToAdd = items.Except(dbItems, comparer).ToList();
                        }
                    }
                    else
                    {
                        cnt++;
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.InnerException?.Message);
                    errorCnt++;
                    cnt++;

                    if (itemCount >= itemsToAdd.Count)
                    {
                        itemCount = itemsToAdd.Count / 2;
                        itemCount = Math.Max(itemCount, 1);
                    }

                    if (errorCnt == 50)
                    {
                        itemCount--;
                        errorCnt = 0;
                        cnt = 0;

                        using (var session = factory.OpenSession())
                        {
                            var dbItems = session.QueryOver<BaseClass>().Where(Restrictions.On<UserRole>(x => x.Id).IsIn(items.Select(i => i.Id).ToArray())).List<BaseClass>();

                            itemsToAdd = items.Except(dbItems, comparer).ToList();
                        }
                    }
                    if (itemCount < 0)
                    {
                        throw new Exception("Unable to restore all items. Trying again or contact support.");
                    }
                }

                if ((itemCount * cnt) > itemsToAdd.Count)
                {
                    cnt = 0;

                    var rnd = new Random();
                    itemsToAdd = itemsToAdd.OrderBy(item => rnd.Next()).ToList(); //randomize the stuff
                }
                nextItems = itemsToAdd.Skip(itemCount * cnt).Take(itemCount).ToList();
            }
        }

        private void InsertItems(IList<BaseClass> items, ISessionFactory factory, int count = 10)
        {
            var failedItems = new List<BaseClass>();
            using (var session = factory.OpenStatelessSession())
            {
                session.SetBatchSize(items.Count);
                foreach (var item in items)
                {
                    session.Insert(item);
                }
            }
        }
    }
}