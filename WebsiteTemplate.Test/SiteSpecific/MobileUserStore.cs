using QBic.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.SiteSpecific
{
    public class MobileUserStore : QBicUserStore<MobileUser>
    {
        public MobileUserStore(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override Task<MobileUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return this.FindByEmailAsync(normalizedUserName, cancellationToken);
        }
    }
}
