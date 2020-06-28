using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public class Student : DynamicClass
    {
        public virtual string Name { get; set; }

        public virtual StudentClass Class { get; set; }
    }
}