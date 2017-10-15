using Microsoft.Practices.Unity;
using Owin;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebsiteTemplate.Models.NonDatabase;

namespace WebsiteTemplate.Utilities
{
    public abstract class ApplicationSettingsCore
    {
        public abstract string GetApplicationName();

        public abstract string ApplicationPassPhrase { get; }

        public abstract Type GetApplicationStartupType { get; }

        public abstract string SystemEmailAddress { get; }

        public virtual List<Assembly> GetAdditinalAssembliesToMap()
        {
            return new List<Assembly>();
        }

        public virtual bool DebugStartup
        {
            get
            {
                return false;
            }
        }

        public virtual string ClientId
        {
            get
            {
                return GetApplicationName();
            }
        }

        /// <summary>
        /// Configure the site specific <see cref="WebsiteTemplate.Models.BaseClass"/> classes for backup processing.
        /// </summary>
        /// <param name="systemTypes"></param>
        public abstract void ConfigureSiteSpecificTypes(Dictionary<int, Type> systemTypes);

        public virtual void PerformAdditionalStartupConfiguration(IAppBuilder app, IUnityContainer container)
        {

        }

        public virtual List<SystemSettingItem> GetAdditionalSystemSettings()
        {
            return new List<SystemSettingItem>();
        }
    }
}