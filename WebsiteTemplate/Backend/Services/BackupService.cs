using NHibernate;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
using WebsiteTemplate.Mappings;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class BackupService
    {
        private DataService DataService { get; set; }

        public BackupService(DataService dataService)
        {
            DataService = dataService;
        }

        public byte[] CreateBackupOfAllData()
        {
            using (var session = DataService.OpenSession())
            {
                var allData = session.QueryOver<BaseClass>().List<BaseClass>().ToList();

                var json = JsonHelper.SerializeObject(allData, false, true);

                var bytes = CompressionHelper.DeflateByte(XXXUtils.GetBytes(json), Ionic.Zlib.CompressionLevel.BestCompression);

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

            using (var session = factory.OpenSession())
            {
                var dbItems = session.QueryOver<BaseClass>().List<BaseClass>();
                foreach (var item in dbItems)
                {
                    session.Delete(item);
                }
                session.Flush();
            }
        }

        public void RestoreBackupOfAllData(byte[] data, string connectionString)
        {
            var tmpBytes = CompressionHelper.InflateByte(data);
            var tmpString = XXXUtils.GetString(tmpBytes);

            var items = JsonHelper.DeserializeObject<List<BaseClass>>(tmpString, true);

            //CreateNewDatabaseSchema(connectionString); //Not sure if this should be an explicit call or not.

            var store = DataStore.GetInstance(null);
            //var connectionString = "Integrated Security=SSPI;Persist Security Info=False;Data Source=localhost;Initial Catalog=WebsiteTemplate";
            var config = store.CreateNewConfigurationUsingConnectionString(connectionString);
            var factory = config.BuildSessionFactory();
            
            var existingCount = 0l;
            var lastCount = -1l;
            while (existingCount < items.Count)
            {
                using (var session = factory.OpenSession())
                {
                    existingCount = session.CreateCriteria<BaseClass>()
                                           .SetProjection(Projections.RowCountInt64())
                                           .List<long>()
                                           .Sum();
                    session.Flush();
                }

                if (existingCount == items.Count)
                {
                    break; // success
                }

                if (lastCount == existingCount)
                {
                    throw new Exception("Error restoring backup, no change after retrying inserting new items.");
                }
                lastCount = existingCount;

                try
                {
                    using (var session = factory.OpenSession())
                    {
                        var existingItems = session.QueryOver<BaseClass>().List<BaseClass>().ToList();
                        SaveItemsToDb(existingItems, items, session); // checks if item already exists, if not, tries to save it. Also has some try-catch processing

                        session.Flush();
                    }
                }
                catch (Exception exception)
                {
                    //Do nothing, just try again.
                }
            }
        }
        private bool SaveItemsToDb(List<BaseClass> existingItems, List<BaseClass> items, ISession session, int count = 10)
        {
            var unsavedList = new List<BaseClass>();
            foreach (var item in items)
            {
                try
                {
                    if (existingItems.Count(e => e.Id == item.Id) == 0)
                    {
                        session.Save(item);
                    }
                }
                catch (Exception exception)
                {
                    unsavedList.Add(item);
                    Console.WriteLine(exception?.Message);
                    Console.WriteLine(exception?.InnerException?.Message);
                }
            }

            if (count < 0)
            {
                throw new Exception("Too many retries");
            }

            if (unsavedList.Count > 0)
            {
                SaveItemsToDb(existingItems, unsavedList, session, --count);
            }

            return true;
        }
    }
}