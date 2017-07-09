using Microsoft.Practices.Unity;
using Owin;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Models.NonDatabase;

namespace WebsiteTemplate.Utilities
{
    public abstract class ApplicationSettingsCore
    {
        public abstract string GetApplicationName();

        public abstract string ApplicationPassPhrase { get; }

        public abstract Type GetApplicationStartupType { get; }

        public abstract string SystemEmailAddress { get; }

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