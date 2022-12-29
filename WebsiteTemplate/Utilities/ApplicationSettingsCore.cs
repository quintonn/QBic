using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebsiteTemplate.Models.NonDatabase;

namespace WebsiteTemplate.Utilities
{
    public abstract class ApplicationSettingsCore : IApplicationSettings
    {
        public ApplicationSettingsCore()
        {

        }

        protected IConfiguration Config { get; set; }
        public void SetConfig(IConfiguration config)
        {
            Config = config;
        }
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

        public virtual void PerformAdditionalStartupConfiguration(IServiceCollection services)
        {

        }

        public virtual List<SystemSettingItem> GetAdditionalSystemSettings(ISession session)
        {
            return new List<SystemSettingItem>();
        }

        public abstract IPersistenceConfigurer GetPersistenceConfigurer(string databaseName);

        public abstract DBProviderType DataProviderType { get; }

        public virtual string AccessControlAllowOrigin { get; } = "*";
        public virtual TimeSpan AccessTokenExpireTimeSpan { get; } = TimeSpan.FromHours(1); //Access token expires after 60min
        public virtual TimeSpan RefreshTokenExpireTimeSpan { get; } = TimeSpan.FromDays(7); //Refresh token expires after 7 days
        public virtual string TokenEndpointPath { get; } = "/api/v1/token";
        public abstract bool TokenEndpointAllowInsecureHttpRequests { get; }
    }
}