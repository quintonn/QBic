using NUnit.Framework;
using System.Linq;
using WebsiteTemplate.UnitTests.Models;

namespace WebsiteTemplate.UnitTests.Tests
{
    [TestFixture]
    internal class TestChild2 : TestBase
    {
        [Test]
        public void AddEmployee1()
        {
            using (var session = DataService.OpenSession())
            {
                var employees = session.QueryOver<Employee>().List().ToList();
                Assert.AreEqual(0, employees.Count);
                var dept = new Department()
                {
                    Name = "Department 2"
                };
                session.SaveOrUpdate(dept);
                var employee = new Employee()
                {
                    Name = "Employee 2",
                    Department = dept
                };
                session.SaveOrUpdate(employee);

                session.Flush();

                employees = session.QueryOver<Employee>().List().ToList();
                Assert.AreEqual(1, employees.Count);
            }
        }
    }
}
