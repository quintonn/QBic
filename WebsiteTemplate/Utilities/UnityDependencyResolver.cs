using log4net;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Unity;
using Unity.Exceptions;

namespace WebsiteTemplate.Utilities
{
    public class UnityDependencyResolver : IDependencyResolver
    {
        private static readonly ILog Logger = SystemLogger.GetLogger<UnityDependencyResolver>();

        protected IUnityContainer Container;

        public UnityDependencyResolver(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            Container = container;
        }

        public IDependencyScope BeginScope()
        {
            var child = Container.CreateChildContainer();
            return new UnityDependencyResolver(child);
        }

        public void Dispose()
        {
            Container?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return Container.Resolve(serviceType);
            }
            catch (ResolutionFailedException exception)
            {
                //Logger.Error("Error resolving " + serviceType.ToString());
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return Container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException exception)
            {
                //Logger.Error("Error resolving " + serviceType.ToString());
                return new List<object>();
            }
        }
    }
}