using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
using WebsiteTemplate.Mappings;
using WebsiteTemplate.Models;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckDefaultValues();

            Console.WriteLine("done");
            Console.ReadKey();
        }

        private static void CheckDefaultValues()
        {
            var store = new DataStore();

            try
            {
                using (var session = store.OpenSession())
                {
                    var tt = typeof(TestChildClass);
                    var properties = typeof(TestChildClass).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Where(zz => zz.GetMethod.IsVirtual)
                                                    .Select(p => p.Name).ToList();

                    var test = session.CreateCriteria<TestChildClass>().List<TestChildClass>();

                    if (test.Count == 0)
                    {
                        var testClass = new TestChildClass();
                        testClass.testColumn = "hello2";
                        testClass.Namex = "nameX";
                        session.Save(testClass);
                    }

                    session.Flush();
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
        }
    }
}
