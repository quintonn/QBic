using QBic.Core.Mappings;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.Mappings
{
    public class MobileUserMap : BaseClassMap<MobileUser>
    {
        public MobileUserMap()
        {
            Table("MobileUser");


            Map(x => x.Email).Not.Nullable();
            Map(x => x.EmailConfirmed);
            Map(x => x.PasswordHash).Not.Nullable();
        }
    }
}
