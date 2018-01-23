using WebsiteTemplate.Models;

namespace WebsiteTemplate.Test.Models
{
    public class UserExtraInfo : DynamicClass
    {
        public virtual User User { get; set; }

        public virtual string ExtraCode { get; set; }
    }
}