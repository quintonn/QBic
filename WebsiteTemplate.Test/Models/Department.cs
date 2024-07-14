using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public class Department : DynamicClass
    {
        public Department()
        {
            Duration = 60; // 60 months
        }

        public virtual string Name { get; set; }

        public virtual int Duration { get; set; }
    }
}
