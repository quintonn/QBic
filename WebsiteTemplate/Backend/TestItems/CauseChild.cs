using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.TestItems
{
    public class CauseChild : DynamicClass
    {
        public virtual string ChildName { get; set; }

        public virtual int SomeInt { get; set; }

        public virtual SuperCause SuperCause { get; set; }
    }
}