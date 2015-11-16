using System;
using WebsiteTemplate.Data;

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
