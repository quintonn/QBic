using NHibernate.Id;
using System;
using NHibernate.Engine;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteTemplate.Models
{
    public class CustomIdentifierGenerator : IIdentifierGenerator
    {
        public object Generate(ISessionImplementor session, object obj)
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<object> GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken)
        {
            return Generate(session, obj);
        }
    }
}