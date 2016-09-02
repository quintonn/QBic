using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;
using WebsiteTemplate.Data;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // from here: http://www.diogonunes.com/blog/calling-a-web-method-in-c-without-a-service-reference/

            var wsdl = "http://www.webservicex.net/globalweather.asmx";

            // another way to do this: http://forums.asp.net/t/1963372.aspx?Dynamic+call+of+SOAP+base+Web+Service+in+net

            var service = new CustomWebService(wsdl);

            service.PreInvoke();

            service.AddParameter("CityName", "London");
            service.AddParameter("CountryName", "");

            try
            {
                service.Invoke("GetWeather");                // name of the WebMethod to call (Case Sentitive again!)
            }
            finally
            {
                service.PosInvoke();
            }

            var result = service.ResultString;

            Console.WriteLine(result);

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

        public ParameterInfo[] ReturnInputParameters(string methodName, Assembly assem)
        {
            //create an instance of the web service type
            //////////////to do/////////////////////////
            //get the name of the web service dynamically from the wsdl
            Object o = assem.CreateInstance("Service");
            Type service = o.GetType();
            ParameterInfo[] paramArr = null;

            //get the list of all public methods available in the generated //assembly
            MethodInfo[] infoArr = service.GetMethods();

            foreach (MethodInfo info in infoArr)
            {
                //get the input parameter information for the
                //required web method
                if (methodName.Equals(info.Name))
                {
                    paramArr = info.GetParameters();
                }
            }

            return paramArr;
        }
    }

}
