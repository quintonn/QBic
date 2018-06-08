using Unity;
using Owin;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebsiteTemplate.Models.NonDatabase;
using log4net.Core;

namespace WebsiteTemplate.Utilities
{
    public abstract class ApplicationSettingsCore
    {
        public abstract string GetApplicationName();

        public abstract string ApplicationPassPhrase { get; }

        public abstract Type GetApplicationStartupType { get; }

        /// <summary>
        /// This is the email address to assign to the 'System' user.
        /// The 'System' user is mostly used for auditing when no actual user is used, such as background processing.
        /// This might be removed in future.
        /// </summary>
        public abstract string SystemEmailAddress { get; }

        public virtual List<Assembly> GetAdditinalAssembliesToMap()
        {
            return new List<Assembly>();
        }

        public virtual Level LogLevel
        {
            get
            {
                return Level.Info;
            }
        }

        public virtual bool DebugStartup
        {
            get
            {
                return false;
            }
        }

        public virtual bool EnableAuditing
        {
            get
            {
                return true;
            }
        }

        public virtual bool ShowSQL
        {
            get
            {
                return false;
            }
        }

        public virtual bool UpdateDatabase
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

        public virtual void PerformAdditionalStartupConfiguration(IAppBuilder app, IUnityContainer container)
        {

        }

        public virtual List<SystemSettingItem> GetAdditionalSystemSettings()
        {
            return new List<SystemSettingItem>();
        }
    }
}