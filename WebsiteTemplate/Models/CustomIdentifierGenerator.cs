using NHibernate.Id;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public class CustomIdentifierGenerator : IIdentifierGenerator
    {
        public object Generate(NHibernate.Engine.ISessionImplementor session, object obj)
        {
            return Guid.NewGuid().ToString();
        }
    }
}