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
                   
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
        }
    }
}
