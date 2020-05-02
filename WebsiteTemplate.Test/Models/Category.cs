using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public class Category : DynamicClass
    {
        public virtual string Name { get; set; }

        public virtual string Description { get; set; }
    }
}