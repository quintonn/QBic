using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate.Exceptions;
using NHibernate.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using QBic.Core.Data;
using QBic.Core.Services;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Models;
using WebsiteTemplate.UnitTests.Models;

namespace WebsiteTemplate.UnitTests
{
    [TestFixture]
    public class DBTests
    {
        private IConfiguration Config { get; set; }
        private IServiceCollection ServiceProvider { get; set; }
        [SetUp]
        public void Setup()
        {
            var types = typeof(User).Assembly.GetTypes().ToList(); // force WebsiteTemplate types to load for datastore initialization
            Console.WriteLine(types.Count);

            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json", true, true);

            Config = config.Build();
            
            var serviceCollection = new ServiceCollection()
            .AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);
            })
            .AddSingleton<IConfiguration>(Config);

            //serviceCollection.configure

            ServiceProvider = serviceCollection;
            serviceCollection.BuildServiceProvider();
        }
        [Test]
        public void DeleteInheritanceTest()
        {
            var store = DataStore.GetInstance(true, false, Config, ServiceProvider);

            using (var session = store.OpenSession())
            {
                var item = new ChildClass()
                {
                    Name = "John",
                    Number = 10,
                };
                session.SaveOrUpdate(item);
                session.Flush();
            }

            using (var session = store.OpenSession())
            {
                var childCount = session.QueryOver<ChildClass>().RowCount();
                var parentCount = session.QueryOver<ParentClass>().RowCount();
                Assert.AreEqual(1, childCount);
                Assert.AreEqual(1, parentCount);

                var child = session.QueryOver<ChildClass>().Take(1).List().ToList().FirstOrDefault();
                // also works if I delete parent class
                session.Delete(child);
                session.Flush();


                childCount = session.QueryOver<ChildClass>().RowCount();
                parentCount = session.QueryOver<ParentClass>().RowCount();
                Assert.AreEqual(0, childCount);
                Assert.AreEqual(0, parentCount);
            }
        }

        [Test]
        public void DeleteTest()
        {
            var store = DataStore.GetInstance(true, false, Config, ServiceProvider);

            using (var session = store.OpenSession())
            {
                session.Query<Department>().Delete();
                session.Query<Employee>().Delete();

                session.Flush();

                var dept = new Department()
                {
                    Name = "Home"
                };
                session.SaveOrUpdate(dept);

                var emply = new Employee()
                {
                    Name = "John",
                    Department = dept
                };

                session.SaveOrUpdate(emply);

                session.Flush();
            }
            
            using (var session = store.OpenSession())
            {
                var demptCount = session.QueryOver<Department>().RowCount();
                var empCount = session.QueryOver<Employee>().RowCount();
                Assert.AreEqual(1, demptCount);
                Assert.AreEqual(1, empCount);

                var dept = session.QueryOver<Department>().Take(1).List().ToList().FirstOrDefault();

                var couldNotDelete = false;
                var deptTable = false;
                try
                {
                    session.Delete(dept);
                    //session.Query<Department>().Delete(); // this will delete without being caught
                    session.Flush();
                }
                catch (GenericADOException ex)
                {
                    couldNotDelete = ex.Message.Contains("could not delete");
                    deptTable = ex.Message.Contains("DELETE FROM Department WHERE");
                }

                // make sure we did get the error
                Assert.IsTrue(couldNotDelete && deptTable);

                // make sure nothing was deleted
                demptCount = session.QueryOver<Department>().RowCount();
                empCount = session.QueryOver<Employee>().RowCount();
                Assert.AreEqual(1, demptCount);
                Assert.AreEqual(1, empCount);
            }
        }

        [Test]
        public void BasicTest()
        {
            var store = DataStore.GetInstance(true, false, Config, ServiceProvider);

            using (var session = store.OpenSession())
            {
                var preCount = session.QueryOver<BasicDataClass>().RowCount();
                if (preCount == 0)
                {
                    var item = new BasicDataClass()
                    {
                        //LongText = "This is some long text",
                        Number = new Random().Next(),
                        Text = "Some text"
                    };
                    session.Save(item);

                    session.Flush();
                }

                var postCount = session.QueryOver<BasicDataClass>().RowCount();
                Assert.AreEqual(postCount, preCount + 1);
            }
        }

        [Test]
        public void AcmeTest()
        {
            var store = DataStore.GetInstance(true, false, Config, ServiceProvider);

            using (var session = store.OpenSession())
            {
                var item = new AcmeData()
                {
                    AccountEmail = "qwe",
                    AccountId = "1",
                    AccountName = "A",
                    AccountType = AcmeAccountType.Production,
                    AgreedToTerms = true,
                    CertificateKey = "x",
                    LocationUrl = "X",
                    PrivateKey = "X",
                    PublicKey = "X",
                    TermsUrl = "X"
                };
                session.Save(item);
                session.Flush();

                var cnt = session.QueryOver<AcmeData>().RowCount();
                Assert.AreEqual(1, cnt);
            }
        }


        [Test]
        public void TestAddingManyItems()
        {
            var store = DataStore.GetInstance(true, false, Config, ServiceProvider);
            var stopwatch = Stopwatch.StartNew();
            var count = 1000;
            using (var session = store.OpenStatelessSession())
            using (var transaction = session.BeginTransaction())
            {
                for (var i = 0; i < count; i++)
                {
                    var dbItem = new BasicDataClass()
                    {
                        LongText = "bla bla bla " + i,
                        Number = i,
                        Text = "more bla bla bla - " + i
                    };
                    session.Insert(dbItem);
                }
                transaction.Commit();
            }
            stopwatch.Stop();
            var elapsedTime = stopwatch.ElapsedMilliseconds;

            Trace.WriteLine($"Adding ${count} items took {elapsedTime} ms");

            Assert.AreEqual(1, 1);
        }

        [Test]
        public void TestMakingAndRestoringBackup()
        {
            //var connection = ConfigurationManager.ConnectionStrings["MainDataStore"];
            var connectionString = Config.GetConnectionString("MainDataStore");
            var store = DataStore.GetInstance(true, false, Config, ServiceProvider);
            var backupService = new BackupService();
            int preCount = 0;
            int postCount = 0;
            using (var session = store.OpenSession())
            {
                preCount = session.QueryOver<ChildClass>().RowCount();
                if (preCount == 0)
                {
                    var item = new ChildClass()
                    {
                        Name = "Test item",
                        Number = 8
                    };
                    session.Save(item);
                    session.Flush();
                    preCount++;
                }
            }

            
            var backup = backupService.CreateFullBackup();


            // Add 1 more item, and we'll make sure it doesn't get removed by the backup using ignoreType
            using (var session = store.OpenSession())
            {
                var item = new ChildClass()
                {
                   Name = "Test item",
                   Number = 50
                };
                session.Save(item);

                session.Flush();
            }

            backupService.RestoreFullBackup(true, backup, connectionString, typeof(ChildClass));

            using (var session = store.OpenSession())
            {
                postCount = session.QueryOver<ChildClass>().RowCount();
            }

            Assert.AreEqual(preCount + 1, postCount);
        }
    }
}