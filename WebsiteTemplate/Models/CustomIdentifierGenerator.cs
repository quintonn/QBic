using NHibernate.Id;
using System;

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