//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;

[assembly: OwinStartup(typeof(BasicAuthentication.Startup.BasicAuthenticationStartup))]
namespace BasicAuthentication.Startup
{
    public class BasicAuthenticationStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var startupType = typeof(IStartup);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var startupTypes = assemblies.SelectMany(s => GetAssemblyTypes(s, startupType));

            var startupInstances = new List<IStartup>();

            foreach (var sType in startupTypes)
            {
                var startupInstance = Activator.CreateInstance(sType) as IStartup;
                startupInstances.Add(startupInstance);
            }

            // Register HttpConfiguration
            var config = new System.Web.Http.HttpConfiguration();

            config.MapHttpAttributeRoutes();

            foreach (var sType in startupInstances)
            {
                sType.Register(config, app);
            }

            // Set up all configuration.
            foreach (var sType in startupInstances)
            {
                sType.Configuration(app);
            }

            app.UseWebApi(config);
        }

        private Type[] GetAssemblyTypes(Assembly assembly, Type startupType)
        {
            try
            {
                var result = assembly.GetTypes().Where(p => startupType.IsAssignableFrom(p) && p.IsInterface == false).ToArray();
                return result;
            }
            catch (Exception err)
            {
                #if (DEBUG)
                    if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
                #endif
                //Console.WriteLine(err.Message);
                return new List<Type>().ToArray();
            }
        } 
    }
}