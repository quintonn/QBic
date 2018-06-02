using Benoni.Core.Data;
using Benoni.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Diagnostics;
using WebsiteTemplate.UnitTests.Models;

namespace WebsiteTemplate.UnitTests
{
    [TestClass]
    public class DBTests
    {
        [TestMethod]
        public void BasicTest()
        {
            var store = DataStore.GetInstance(true);

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

        [TestMethod]
        public void AcmeTest()
        {
            var store = DataStore.GetInstance(true);

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


        [TestMethod]
        public void TestAddingManyItems()
        {
            var store = DataStore.GetInstance(true);
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

        [TestMethod]
        public void TestMakingAndRestoringBackup()
        {
            var connection = ConfigurationManager.ConnectionStrings["MainDataStore"];
            var store = DataStore.GetInstance(true);
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

            var backupService = new BackupService();
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

            backupService.RestoreFullBackup(true, backup, connection.ConnectionString, typeof(ChildClass));

            using (var session = store.OpenSession())
            {
                postCount = session.QueryOver<ChildClass>().RowCount();
            }

            Assert.AreEqual(preCount + 1, postCount);
        }
    }
}