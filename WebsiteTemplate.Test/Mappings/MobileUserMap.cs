using QBic.Core.Mappings;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.Mappings
{
    public class MobileUserMap : BaseClassMap<MobileUser>
    {
        public MobileUserMap()
        {
            Table("MobileUser");
            

            Map(x => x.UserName).Not.Nullable(); // don't commit. // find out why it doesn't ignore this field

            Map(x => x.Email).Not.Nullable();
            Map(x => x.EmailConfirmed);
            Map(x => x.PasswordHash).Not.Nullable();
        }
    }
}
