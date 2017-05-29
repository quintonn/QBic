using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.TestItems
{
    public abstract class TestClassA : DynamicClass
    {
        public virtual string Name { get; set; }

        public abstract int BaseValue { get; }
    }
}