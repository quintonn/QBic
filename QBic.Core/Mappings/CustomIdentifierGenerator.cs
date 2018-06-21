using NHibernate.Engine;
using NHibernate.Id;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QBic.Core.Mappings
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