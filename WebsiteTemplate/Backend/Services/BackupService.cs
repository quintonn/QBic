using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.TestItems;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
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
                SystemTypes.Add(6, typeof(Models.User));
                SystemTypes.Add(7, typeof(BackgroundJobResult));
                SystemTypes.Add(8, typeof(CauseChild));
                SystemTypes.Add(9, typeof(SuperCause));
                SystemTypes.Add(10, typeof(Models.SystemSettings));

                AppSettings.ConfigureSiteSpecificTypes(SystemTypes);
            }
        }

        private List<BaseClass> GetItems(Type type, ISession session, int skip = 0, int take = int.MaxValue)
        {
            var queryOverMethodInfo = typeof(ISession).GetMethods().FirstOrDefault(m => m.Name == "QueryOver"
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

        public byte[] CreateBackupOfAllData(BackupType backupType = BackupType.Unknown)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
            if (connectionString.Contains("##CurrentDirectory##"))
            {
                var currentDirectory = HttpRuntime.AppDomainAppPath;
                var path = connectionString.Split(";".ToCharArray()).First();
                path = path.Split("\\".ToCharArray()).Last();
                var filePath = currentDirectory + "\\Data\\" + path;
                DataStore.KillConnection();

                var data = File.ReadAllBytes(filePath);

                var bytes = CompressionHelper.DeflateByte(data, Ionic.Zlib.CompressionLevel.BestCompression);
                return bytes;
            }
            else  if (backupType == BackupType.SqlFullBackup)
            {
                var bytes = CreateSqlBackup(connectionString);
                bytes = CompressionHelper.DeflateByte(bytes, Ionic.Zlib.CompressionLevel.BestCompression);
                return bytes;
            }

            #region JSON Backup
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

                                var items = GetItems(type, session, added, nextLoad);

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
            #endregion
        }

        private byte[] CreateSqlBackup(string connectionString)
        {
            var currentDirectory = HttpRuntime.AppDomainAppPath;
            var tempFolder = currentDirectory + "Temp";
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            var srv = new Server();
            var cnt = srv.Databases.Count;

            var conString = new SqlConnectionStringBuilder(connectionString);
            var db = srv.Databases[conString.InitialCatalog];

            var bk = new Backup();
            bk.Action = BackupActionType.Database;
            bk.BackupSetDescription = "Website Template backup";
            bk.BackupSetName = "WebsiteTempbackup";
            bk.Database = conString.InitialCatalog;

            // Declare a BackupDeviceItem by supplying the backup device file name in the constructor, and the type of device is a file.   
            //var bdi = default(BackupDeviceItem);
            var filePath = tempFolder + @"\backup_Test.bak";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var bdi = new BackupDeviceItem(filePath, DeviceType.File);

            // Add the device to the Backup object.   
            bk.Devices.Add(bdi);
            // Set the Incremental property to False to specify that this is a full database backup.   
            bk.Incremental = false;

            // Set the expiration date.   
            bk.ExpirationDate = new DateTime(2020, 10, 5);

            // Specify that the log must be truncated after the backup is complete.   
            bk.LogTruncation = BackupTruncateLogType.NoTruncate;

            // Run SqlBackup to perform the full database backup on the instance of SQL Server.   
            bk.SqlBackup(srv);

            // Backup is done

            // Remove the backup device from the Backup object.   
            bk.Devices.Remove(bdi);

            var bytes = File.ReadAllBytes(filePath);
            return bytes;
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

            //IList<BaseClass> items = null;
            //using (var session = factory.OpenSession())
            //{
            //    items = session.QueryOver<BaseClass>().List<BaseClass>();
            //}

            //Delete(dbItems, factory);

            var comparer = new BasicClassEqualityComparer();

            var ids = SystemTypes.Keys.ToList().OrderBy(i => i);

            foreach (var id in ids)
            {
                var type = SystemTypes[id];
                List<BaseClass> items;
                using (var session = DataService.OpenSession())
                {
                    items = GetItems(type, session);
                }
                Delete(items, factory);
            }

            //    var users = items.Where(i => i is Models.User).ToList();
            //var userRoles = items.Where(i => i is UserRole).ToList();
            //var eventRoleItems = items.Where(i => i is EventRoleAssociation).ToList();
            //var auditEvents = items.Where(i => i is AuditEvent).ToList();
            //var otherItems = items.Except(users, comparer)
            //                      .Except(userRoles, comparer)
            //                      .Except(eventRoleItems, comparer)
            //                      .Except(auditEvents, comparer)
            //                      .ToList();

            //Delete(otherItems, factory);
            //Delete(eventRoleItems, factory);
            //Delete(auditEvents, factory);
            //Delete(userRoles, factory);
            //Delete(users, factory);

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

                var users = items.Where(i => i is Models.User).ToList();
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

        public void RestoreSqlDatabase(byte[] data, string connectionString, string databaseName, bool overrideExistingDatabase)
        {
            var currentDirectory = HttpRuntime.AppDomainAppPath;
            var tempFolder = currentDirectory + "Temp";
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            var filePath = tempFolder + "\\" + Guid.NewGuid().ToString();

            var dbName = databaseName;

            DataStore.KillConnection();

            var connection = new SqlConnectionStringBuilder(connectionString);
            if (connection.InitialCatalog == dbName)
            {
                overrideExistingDatabase = true;
            }
            if (overrideExistingDatabase == true)
            {
                dbName = connection.InitialCatalog;
            }
            connection.InitialCatalog = "master";
            var tmp = connection.ToString();

            using (var sqlConnection = new SqlConnection(tmp))
            {
                var serverConnection = new ServerConnection(sqlConnection);
                sqlConnection.Open();
                if (overrideExistingDatabase == true)
                {
                    using (var sqlcommand = new SqlCommand("ALTER DATABASE " + dbName + " SET Single_User WITH Rollback IMMEDIATE", sqlConnection))
                    {
                        sqlcommand.ExecuteNonQuery();
                    }
                }
                try
                {
                    var server = new Server(serverConnection);
                    var restore = new Restore();

                    var base64 = XXXUtils.GetString(data);
                    base64 = base64.Replace("data:;base64,", "").Replace("\0", "");
                    var bytes = Convert.FromBase64String(base64);
                    bytes = CompressionHelper.InflateByte(bytes, Ionic.Zlib.CompressionLevel.BestCompression);
                    File.WriteAllBytes(filePath, bytes);

                    restore.Devices.AddDevice(filePath, DeviceType.File);

                    // Specify the database name.   
                    restore.Action = RestoreActionType.Database;

                    var dbFile = new RelocateFile();
                    var logFile = new RelocateFile();

                    var dataTable = restore.ReadFileList(server);

                    restore.Database = dbName;

                    if (overrideExistingDatabase == false)
                    {
                        dbFile.LogicalFileName = getRealLogicalName(dataTable, false);
                        logFile.LogicalFileName = getRealLogicalName(dataTable, true);

                        dbFile.PhysicalFileName = server.Databases[0].FileGroups[0].Files[0].FileName.Replace(server.Databases[0].Name, databaseName);
                        logFile.PhysicalFileName = server.Databases[0].LogFiles[0].FileName.Replace(server.Databases[0].Name, databaseName);

                        restore.RelocateFiles.Add(dbFile);
                        restore.RelocateFiles.Add(logFile);
                    }

                    restore.ReplaceDatabase = true;
                    restore.NoRecovery = false;
                    // Restore the full database backup with no recovery.   
                    restore.SqlRestore(server);

                    // Restore done

                    // Remove the device from the Restore object.  
                    restore.Devices.RemoveAt(0);

                    // Set the NoRecovery property to False.   

                }
                finally
                {
                    if (overrideExistingDatabase == true)
                    {
                        using (var sqlcommand = new SqlCommand("ALTER DATABASE [" + dbName + "] SET Multi_User", sqlConnection))
                        {
                            sqlcommand.ExecuteNonQuery();
                        }
                    }
                }

                sqlConnection.Close();
            }
        }

        private static string getRealLogicalName(DataTable dt, bool isLogFile)
        {
            var realName = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (isLogFile == true && dt.Rows[i]["Type"].ToString() == "L")
                {
                    realName = dt.Rows[i]["LogicalName"].ToString();
                    break;
                }
                else if (isLogFile == false && dt.Rows[i]["Type"].ToString() == "D")
                {
                    realName = dt.Rows[i]["LogicalName"].ToString();
                    break;
                }
            }

            return realName;
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