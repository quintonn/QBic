using BasicAuthentication.Users;
using Microsoft.Owin.Security.DataProtection;
using WebsiteTemplate.Data;

namespace WebsiteTemplate.SiteSpecific.DefaultsForTest
{
    public class DefaultUserManager : CoreUserManager
    {
        public DefaultUserManager(UserContext userContext, IDataProtectionProvider dataProtectionProvider)
            :base(userContext, dataProtectionProvider)
        {

        }
    }
}