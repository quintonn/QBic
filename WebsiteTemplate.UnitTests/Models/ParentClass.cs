using QBic.Core.Models;

namespace WebsiteTemplate.UnitTests.Models
{
    public abstract class ParentClass : DynamicClass
    {
        public virtual string Name { get; set; }
        public abstract int BaseValue { get; }
    }
}
