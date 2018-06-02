using Benoni.Core.Models;

namespace WebsiteTemplate.Models
{
    public class UserRole : DynamicClass
    {
        public virtual string Name { get; set; }

        public virtual string Description { get; set; }
    }
}