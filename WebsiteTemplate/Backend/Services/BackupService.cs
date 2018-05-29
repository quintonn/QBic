using log4net;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
using WebsiteTemplate.Data.BaseTypes;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

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

        private static readonly ILog Logger = SystemLogger.GetLogger<BackupService>();
        public static bool BusyWithBackups { get; set; } = false;

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
                var cnt = 1;

                var types = XXXUtils.GetAllBaseClassTypes(appSettings);
                foreach (var type in types)
                {
                    SystemTypes.Add(cnt++, type);
                }
            }
        }

        public void DeleteAllOfType<T>(IStatelessSession session)
        {
            session.Query<T>().Delete();
        }

        private int GetCount(Type type, ISession session)
        {
            var queryOverMethodInfo = typeof(ISession).GetMethods().FirstOrDefault(m => m.Name == "QueryOver"
                                                                && m.GetParameters().Count() == 0);
            var queryOverMethod = queryOverMethodInfo.MakeGenericMethod(type);

            var genericType = typeof(IQueryOver<>);
            Type[] typeArgs = { type };

            var genericType1 = typeof(IQueryOver<,>);
            Type[] typeArgs1 = { type, type };

            var queryOver = queryOverMethod.Invoke(session, null);

            var gType = genericType.MakeGenericType(typeArgs);
            var gType1 = genericType1.MakeGenericType(typeArgs1);

            var selectMethod = gType1.GetMethods().Where(m => m.Name == "Select").Last();
            var parameters = new IProjection[] { Projections.RowCount() };
            queryOver = selectMethod.Invoke(queryOver, new object[] { parameters });

            var futureValueMethodInfo = gType.GetMethods().Where(m => m.Name == "FutureValue").Last();
            var futureValueMethod = futureValueMethodInfo.MakeGenericMethod(typeof(int));

            var futureValue = futureValueMethod.Invoke(queryOver, null) as IFutureValue<int>;

            var count = futureValue.Value;

            return count;
        }

        private List<BaseClass> GetItems(Type type, IStatelessSession session, int skip = 0, int take = int.MaxValue)
        {
            var queryOverMethodInfo = typeof(IStatelessSession).GetMethods().FirstOrDefault(m => m.Name == "QueryOver"
                                                                                                 && m.GetParameters().Count() == 0);
            var queryOverMethod = queryOverMethodInfo.MakeGenericMethod(type);

            var genericType = typeof(IQueryOver<>);
            Type[] typeArgs = { type };

            var queryOver = queryOverMethod.Invoke(session, null);

            var gType = genericType.MakeGenericType(typeArgs);

            var skipMethod = gType.GetMethod("Skip");
            queryOver = skipMethod.Invoke(queryOver, new object[] { skip });

            var takeMethod = gType.GetMethod("Take");
            queryOver = takeMethod.Invoke(queryOver, new object[] { take });

            var list = gType.GetMethods().FirstOrDefault(m => m.Name == "List"
                                            && m.GetParameters().Count() == 0);

            var enumerableItems = list.Invoke(queryOver, null) as IEnumerable;
            var items = enumerableItems.OfType<BaseClass>().ToList();

            return items;
        }

        private void CreateBackupFile(string backupLocation)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WebsiteTemplate.Data.BlankDB.db";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var backupStream = File.Create(backupLocation))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(backupStream);
            }
        }

        public byte[] CreateFullBackup()
        {
            var cnt = 1;
            var backupName = "Backup_" + DateTime.Now.ToString("dd_MM_yyyy") + ".db";
            var currentDirectory = HttpRuntime.AppDomainAppPath + "\\Data\\";
            while (File.Exists(currentDirectory + backupName))
            {
                backupName = "Backup_" + DateTime.Now.ToString("dd_MM_yyyy") + "_" + cnt + ".db";
                cnt++;
            }

            try
            {
                DynamicClass.SetIdsToBeAssigned = true;

                Logger.Info("About to create backup");

                CreateBackupFile(currentDirectory + backupName);

                var connectionString = String.Format(@"Data Source=##CurrentDirectory##\Data\{0};Version=3;Journal Mode=Off;Connection Timeout=12000", backupName);
                var store = DataStore.GetInstance(null);
                var config = store.CreateNewConfigurationUsingConnectionString(connectionString, String.Empty);
                new SchemaUpdate(config).Execute(false, true); // Build the tables etc.
                var factory = config.BuildSessionFactory();

                var ids = SystemTypes.Keys.ToList().OrderBy(i => i);

                var total = 0;

                using (var backupSession = factory.OpenStatelessSession())
                using (var session = DataService.OpenStatelessSession())
                {
                    foreach (var id in ids)
                    {
                        var type = SystemTypes[id];
                        Logger.Info("Backing up " + type.ToString());

                        var items = GetItems(type, session);
                        Logger.Info("Got items");

                        var sameTypeProperties = type.GetProperties().Where(p => p.PropertyType == type).ToList();
                        if (sameTypeProperties.Count > 0)
                        {
                            var totalItemsToAdd = items.Where(i => i.GetType() == type).ToList();

                            var addedItems = new List<string>();
                            while (addedItems.Count < totalItemsToAdd.Count)
                            {
                                foreach (var prop in sameTypeProperties)
                                {
                                    var itemsToAdd = totalItemsToAdd.Where(i => ShouldAddItem(i, prop, totalItemsToAdd, addedItems) == true).ToList();

                                    total += itemsToAdd.Count();
                                    InsertItems(itemsToAdd, factory, type);
                                    addedItems.AddRange(itemsToAdd.Select(i => i.Id));
                                }
                            }
                        }
                        else
                        {
                            var itemsToAdd = items.Where(i => i.GetType() == type).ToList();

                            total += itemsToAdd.Count();
                            InsertItems(itemsToAdd, factory, type);
                        }
                        Logger.Info("Items added to backup");
                    }
                }

                Logger.Info("Closing factory");
                factory.Close();

                using (var session = factory.OpenSession())
                {
                    var users = session.QueryOver<Models.User>().List().ToList();
                    var count = session
                            .CreateCriteria<BaseClass>()
                            .SetProjection(
                                Projections.Count(Projections.Id())
                            )
                            .List<int>()
                            .Sum();

                    if (count != total)
                    {
                        Logger.Info("Backup did not complete successfully.");
                        throw new Exception("Backup did not complete successfully. Try again or contact support.");
                    }
                }
                Logger.Info("Closing store session");
                store.CloseSession();

                return File.ReadAllBytes(currentDirectory + backupName);
            }
            catch (Exception e)
            {
                SystemLogger.LogError<BackupService>("Error creating full backup", e);
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                DynamicClass.SetIdsToBeAssigned = true;

                File.Delete(currentDirectory + backupName);
            }
        }

        public void CreateNewDatabaseSchema(string connectionString, string providerName)
        {
            var store = DataStore.GetInstance(null);

            DynamicClass.SetIdsToBeAssigned = true; // This will set the Fluent NHibernate mappings' id's to be assigned and not GUID's for the restore.

            var config = store.CreateNewConfigurationUsingConnectionString(connectionString, providerName);
            new SchemaUpdate(config).Execute(true, true);

            DynamicClass.SetIdsToBeAssigned = false; // Change it back
        }

        public void RemoveExistingData(string connectionString, bool restoreSystemSettings)
        {
            var ids = SystemTypes.Keys.ToList().OrderBy(i => i).Reverse().ToList();

            var itemsAllowed = 0;

            var deleteMethodInfo = GetType().GetMethods().FirstOrDefault(m => m.Name == "DeleteAllOfType"
                                                                && m.GetParameters().Count() == 1);

            using (var session = DataService.OpenStatelessSession())
            using (var transaction = session.BeginTransaction())
            {
                foreach (var id in ids)
                {
                    var type = SystemTypes[id];

                    if ((type == typeof(Models.SystemSettings) || (type == typeof(SystemSettingValue))) && restoreSystemSettings == false)
                    {
                        itemsAllowed = session.QueryOver<Models.SystemSettings>().RowCount() +
                                       session.QueryOver<Models.SystemSettingValue>().RowCount();
                        continue;
                    }

                    var deleteMethod = deleteMethodInfo.MakeGenericMethod(type);
                    deleteMethod.Invoke(this, new object[] { session });
                }
                transaction.Commit();
            }

            using (var session = DataService.OpenSession())
            {
                var count = session
                        .CreateCriteria<BaseClass>()
                        .SetProjection(
                            Projections.Count(Projections.Id())
                        )
                        .List<int>()
                        .Sum();
                if (count != itemsAllowed)
                {
                    throw new Exception("Not all items were deleted. Contact support");
                }
            }
        }

        private bool ShouldAddItem(BaseClass item, PropertyInfo propertyInfo, IList<BaseClass> allItems, List<string> addedItems)
        {
            if (addedItems.Contains(item.Id))
            {
                return false;
            }

            var sameTypeProperty = propertyInfo.GetValue(item) as BaseClass;
            if (sameTypeProperty == null)
            {
                return true;
            }

            if (addedItems.Contains(sameTypeProperty.Id))
            {
                return true;
            }

            return false;
        }

        private bool IsChildOf(BaseClass item, PropertyInfo prop, string itemId)
        {
            var value = prop.GetValue(item) as BaseClass;
            if (value == null)
            {
                return false;
            }

            if (value.Id == itemId)
            {
                return true;
            }

            return false;
        }

        public bool RestoreFullBackup(byte[] data, string dbConnectionString, string providerName, bool restoreSystemSettings)
        {
            var cnt = 1;
            var backupName = "Restore_" + DateTime.Now.ToString("dd_MM_yyyy") + ".db";
            var currentDirectory = HttpRuntime.AppDomainAppPath + "\\Data\\";
            if (!Directory.Exists(currentDirectory))
            {
                Directory.CreateDirectory(currentDirectory);
            }
            while (File.Exists(currentDirectory + backupName))
            {
                backupName = "Restore_" + DateTime.Now.ToString("dd_MM_yyyy") + "_" + cnt + ".db";
                cnt++;
            }

            try
            {
                File.WriteAllBytes(currentDirectory + backupName, data);

                var connectionString = String.Format(@"Data Source=##CurrentDirectory##\Data\{0};Version=3;Journal Mode=Off;Connection Timeout=12000", backupName);
                var store = DataStore.GetInstance(null);
                var backupConfig = store.CreateNewConfigurationUsingConnectionString(connectionString, String.Empty);
                var backupFactory = backupConfig.BuildSessionFactory();

                DynamicClass.SetIdsToBeAssigned = true;
                var config = store.CreateNewConfigurationUsingConnectionString(dbConnectionString, providerName);
                var factory = config.BuildSessionFactory();

                var ids = SystemTypes.Keys.ToList().OrderBy(i => i);

                var totalItems = 0;

                using (var backupSession = backupFactory.OpenStatelessSession())
                using (var session = factory.OpenStatelessSession())
                {
                    foreach (var id in ids)
                    {
                        var type = SystemTypes[id];

                        if ((type == typeof(Models.SystemSettings) || (type == typeof(SystemSettingValue))) && restoreSystemSettings == false)
                        {
                            var tmpItems = GetItems(type, backupSession);

                            continue;
                        }

                        var items = GetItems(type, backupSession);

                        var sameTypeProperties = type.GetProperties().Where(p => p.PropertyType == type).ToList();
                        if (sameTypeProperties.Count > 0)
                        {
                            var totalItemsToAdd = items.Where(i => i.GetType() == type).ToList();

                            var addedItems = new List<string>();
                            while (addedItems.Count < totalItemsToAdd.Count)
                            {
                                foreach (var prop in sameTypeProperties)
                                {
                                    var itemsToAdd = totalItemsToAdd.Where(i => ShouldAddItem(i, prop, totalItemsToAdd, addedItems) == true).ToList();

                                    totalItems += itemsToAdd.Count();

                                    InsertItems(itemsToAdd, factory, type);
                                    addedItems.AddRange(itemsToAdd.Select(i => i.Id));
                                }
                            }
                        }
                        else
                        {
                            var itemsToAdd = items.Where(i => i.GetType() == type).ToList();

                            totalItems += itemsToAdd.Count();

                            InsertItems(itemsToAdd, factory, type);
                        }
                    }
                }

                factory.Close();
                backupFactory.Close();

                using (var session = factory.OpenSession())
                {
                    var count = session
                            .CreateCriteria<BaseClass>()
                            .SetProjection(
                                Projections.Count(Projections.Id())
                            )
                            .List<int>()
                            .Sum();

                    if (restoreSystemSettings == false)
                    {
                        var settingsCount = session.QueryOver<Models.SystemSettings>().RowCount() +
                                            session.QueryOver<SystemSettingValue>().RowCount();
                        count -= settingsCount;
                    }

                    if (count != totalItems)
                    {
                        return false;
                    }
                }

                store.CloseSession();

                return true;
            }
            catch (Exception e)
            {
                SystemLogger.LogError<BackupService>("Error restoring full backup", e);
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                DynamicClass.SetIdsToBeAssigned = true;
                if (File.Exists(currentDirectory + backupName))
                {
                    File.Delete(currentDirectory + backupName);
                }
            }
        }

        private void InsertItems(IList<BaseClass> items, ISessionFactory factory, Type type)
        {
            var failedItems = new List<BaseClass>();
            var longStringProperties = type.GetProperties().Where(p => p.PropertyType == typeof(LongString)).ToList();
            var hasLongStringProperty = longStringProperties.Count() > 0;
            using (var session = factory.OpenStatelessSession())
            using (var transaction = session.BeginTransaction())
            {
                foreach (var item in items)
                {
                    if (hasLongStringProperty)
                    {
                        foreach (var prop in longStringProperties)
                        {
                            var value = prop.GetValue(item);
                            if (value == null || (value as LongString).Base == null)
                            {
                                prop.SetValue(item, new LongString(String.Empty));
                            }
                        }
                    }
                    session.Insert(item);
                }
                transaction.Commit();
            }
        }
    }
}