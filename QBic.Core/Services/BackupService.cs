﻿using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using QBic.Core.Data;
using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;
using QBic.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QBic.Core.Services
{
    public class BackupService
    {
        private static Dictionary<int, Type> SystemTypes { get; set; }

        private static readonly ILogger Logger = SystemLogger.GetLogger<BackupService>();
        public static bool BusyWithBackups { get; set; } = false;

        private DataStore DataService { get; set; }

        public static readonly string BACKUP_HEADER_KEY = "BackupType";

        private IApplicationSettings AppSettings;

        public BackupService(IApplicationSettings appSettings)
        {
            AppSettings = appSettings;
            DataService = DataStore.GetInstance(false, appSettings, null);

            if (SystemTypes == null)
            {
                SystemTypes = new Dictionary<int, Type>();
                var cnt = 1;

                var types = GetAllBaseClassTypes();
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

        private List<BaseClass> GetItems<Session>(Type type, Session session, int skip = 0, int take = int.MaxValue)
        {
            var queryOverMethodInfo = session.GetType().GetMethods().FirstOrDefault(m => m.Name == "QueryOver"
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
            var resourceName = "QBic.Core.Data.BlankDB.db";

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

            var currentDirectory = QBicUtils.GetCurrentDirectory() + "\\Data\\";
            if (!Directory.Exists(currentDirectory))
            {
                Directory.CreateDirectory(currentDirectory); // try create the directory if it doesn't exist, else it would crash anyway
            }
            
            while (File.Exists(currentDirectory + backupName))
            {
                backupName = "Backup_" + DateTime.Now.ToString("dd_MM_yyyy") + "_" + cnt + ".db";
                cnt++;
            }

            try
            {
                Logger.LogInformation("About to create backup");

                CreateBackupFile(currentDirectory + backupName);

                var connectionString = String.Format(@"Data Source=##CurrentDirectory##\Data\{0};Version=3;Journal Mode=Off;Connection Timeout=12000", backupName);
                //var connectionString = $"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared;Version=3;New=True";
                // this doesn't work because the code is reading the sqlite db file at the end. Maybe there is another way to get it
                var store = DataStore.GetInstance(false, AppSettings, null);
                var tmp = DataStore.DbProviderType;
                DataStore.DbProviderType = DBProviderType.SQLITE;
                var config = store.CreateNewConfigurationUsingConnectionString(connectionString);
                new SchemaUpdate(config).Execute(false, true); // Build the tables etc.
                var factory = config.BuildSessionFactory();

                DataStore.DbProviderType = tmp;

                var ids = SystemTypes.Keys.ToList().OrderBy(i => i);

                var total = 0;

                using (var backupSession = factory.OpenStatelessSession())
                using (var session = DataService.OpenStatelessSession())
                {
                    foreach (var id in ids)
                    {
                        var type = SystemTypes[id];
                        Logger.LogInformation("Backing up " + type.ToString());

                        var items = GetItems(type, session);
                        Logger.LogInformation("Got items");

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
                        Logger.LogInformation("Items added to backup");
                    }
                }

                Logger.LogInformation("Closing factory");

                using (var session = factory.OpenSession())
                {
                    var count = session
                            .CreateCriteria<BaseClass>()
                            .SetProjection(
                                Projections.Count(Projections.Id())
                            )
                            .List<int>()
                            .Sum();

                    if (count != total)
                    {
                        Logger.LogInformation("Backup did not complete successfully.");
                        throw new Exception("Backup did not complete successfully. Try again or contact support.");
                    }
                }
                Logger.LogInformation("Closing store session");

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
                File.Delete(currentDirectory + backupName);
            }
        }

        private void RemoveExistingData(params Type[] typesToIgnore)
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

                    if (typesToIgnore.Contains(type))
                    {
                        itemsAllowed += GetItemCount(type, session);
                        continue;
                    }

                    if (type.FullName == "WebsiteTemplate.Models.AuditEvent")
                    {
                        continue;
                    }

                    Logger.LogInformation($"Deleting items of type {type.Name}");
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

        public bool RestoreFullBackup(bool removeExistingItemsFirst, byte[] data, params Type[] typesToIgnore)
        {
            Logger.LogInformation("Restoring full backup");
            if (removeExistingItemsFirst)
            {
                // TODO: without transaction scopes, this is very bad. 
                // see if it's possible to restore to a new temp database and then switch the databases, or something like that

                Logger.LogInformation("Backup Restore - Removing existing data");
                RemoveExistingData(typesToIgnore);
                Logger.LogInformation("Existing data removed");
            }

            var stopwatch = new Stopwatch();
            var cnt = 1;
            var backupName = "Restore_" + DateTime.Now.ToString("dd_MM_yyyy") + ".db";
            var currentDirectory = QBicUtils.GetCurrentDirectory() + "\\Data\\";
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
                Logger.LogInformation("Inside restore try");
                File.WriteAllBytes(currentDirectory + backupName, data);

                var connectionString = String.Format(@"Data Source=##CurrentDirectory##\Data\{0};Version=3;Journal Mode=Off;Connection Timeout=12000", backupName);
                var store = DataStore.GetInstance(false, AppSettings, null);
                var backupConfig = store.CreateNewConfigurationUsingConnectionString(connectionString);
                var backupFactory = backupConfig.BuildSessionFactory();

                var config = store.CreateNewConfigurationUsingDatabaseName("MainDataStore");
                var factory = config.BuildSessionFactory();

                var ids = SystemTypes.Keys.ToList().OrderBy(i => i);

                var totalItems = 0;

                using (var backupSession = backupFactory.OpenStatelessSession())
                using (var session = factory.OpenStatelessSession())
                {
                    Logger.LogInformation($"id count = {ids.Count()}");
                    foreach (var id in ids)
                    {
                        var type = SystemTypes[id];

                        Logger.LogInformation($"Processing type {type?.Name}");
                        if (typesToIgnore.Contains(type))
                        {
                            continue;
                        }

                        List<BaseClass> items;
                        try
                        {
                            items = GetItems(type, backupSession);
                        }
                        catch (Exception error)
                        {
                            // This could happen if there are new classes in the code since the backup has been made
                            Logger.LogError(error, $"Error getting items of type {type.Name}: {error.Message}, skipping restore of this type");
                            continue;
                        }

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

                Logger.LogInformation("Done with id look, verifying number of records restored");
                using (var session = factory.OpenSession())
                {
                    var count = session
                            .CreateCriteria<BaseClass>()
                            .SetProjection(
                                Projections.Count(Projections.Id())
                            )
                            .List<int>()
                            .Sum();

                    var queryOverMethodInfo = typeof(ISession).GetMethods().FirstOrDefault(m => m.Name == "QueryOver" && m.GetParameters().Count() == 0);
                    foreach (var t in typesToIgnore)
                    {
                        var tCount = GetItemCount(t, session);
                        count -= tCount;
                    }

                    if (count != totalItems)
                    {
                        return false;
                    }
                }

                stopwatch.Stop();

                Logger.LogInformation("Full restore took " + stopwatch.ElapsedMilliseconds + " ms");

                factory.Close();

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
                if (File.Exists(currentDirectory + backupName))
                {
                    File.Delete(currentDirectory + backupName);
                }
            }
        }

        private int GetItemCount<Session>(Type t, Session session)
        {
            var queryOverMethodInfo = session.GetType().GetMethods().FirstOrDefault(m => m.Name == "QueryOver"
                                                                && m.GetParameters().Count() == 0);
            var queryOverMethod = queryOverMethodInfo.MakeGenericMethod(t);

            var genericType = typeof(IQueryOver<>);
            Type[] typeArgs = { t };

            var queryOver = queryOverMethod.Invoke(session, null);

            var gType = genericType.MakeGenericType(typeArgs);

            var rowCountMethod = gType.GetMethod("RowCount");
            var tCountObj = rowCountMethod.Invoke(queryOver, null);

            return (int)tCountObj;
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
                            if (value == null || String.IsNullOrWhiteSpace((value as LongString)?.ToString()))
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

        private List<Type> GetAllBaseClassTypes()
        {
            var tmpAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = new List<Type>();
            foreach (var assembly in tmpAssemblies)
            {
                if (assembly.FullName.Contains("System.Data.SqlClient"))
                {
                    continue; // might be cached locally after upgrade, might not be a problem
                }
                var tmpBaseTypes = assembly.GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(BaseClass)) && t.IsAbstract == false).ToList();
                allTypes.AddRange(tmpBaseTypes);
            }

            var result = new List<Type>();

            foreach (var type in allTypes)
            {
                ProcessType(type, result);
            }

            return result;
        }

        private static List<Type> ProcessingTypes { get; set; } = new List<Type>();

        private static void ProcessType(Type type, List<Type> sortedTypes)
        {
            /* Only process BaseClass classes and don't repeat any */
            if (!type.IsSubclassOf(typeof(BaseClass)) || sortedTypes.Contains(type))
            {
                return;
            }

            /* Classes to explicitly ignore */
            if (type == typeof(DynamicClass))
            {
                return;
            }

            if (ProcessingTypes.Contains(type))
            {
                return;
            }
            ProcessingTypes.Add(type);

            var properties = type.GetProperties().Where(p => p.PropertyType.IsClass &&
                                                             p.PropertyType.IsSubclassOf(typeof(BaseClass)) &&
                                                             p.PropertyType != type).ToList();
            foreach (var property in properties)
            {
                var pType = property.PropertyType;
                ProcessType(pType, sortedTypes);
            }

            ProcessingTypes.Remove(type);
            sortedTypes.Add(type);
        }

    }
}