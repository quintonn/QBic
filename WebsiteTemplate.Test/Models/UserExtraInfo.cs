using QBic.Core.Models;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Test.Models
{
    public class UserExtraInfo : DynamicClass
    {
        public virtual User User { get; set; }

        public virtual string ExtraCode { get; set; }

        public virtual TestUserRole UserRole { get; set; }
    }
}