using QBic.Core.Models;

namespace WebsiteTemplate.UnitTests.Models
{
    public class Employee : DynamicClass
    {
        public virtual string Name { get; set; }

        public virtual Department Department { get; set; }
    }
}
