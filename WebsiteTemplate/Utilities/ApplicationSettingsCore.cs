using System;
using System.Collections.Generic;

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
    }
}