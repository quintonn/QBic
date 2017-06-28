using WebsiteTemplate.Models;

namespace WebsiteTemplate.Test.MenuItems
{
    public abstract class TestClassA : DynamicClass
    {
        public virtual string Name { get; set; }

        public abstract int BaseValue { get; }
    }
}