﻿using Microsoft.Practices.Unity;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.SiteSpecific.Utilities
{
    public interface IApplicationSettings
    {
        string GetApplicationName();

        void RegisterUnityContainers(IUnityContainer container);

        void SetupDefaults(ISession session);
    }
}