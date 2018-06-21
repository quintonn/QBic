using QBic.Core.Models;

namespace WebsiteTemplate.Test.MenuItems
{
    public class CauseChild : DynamicClass
    {
        public virtual string ChildName { get; set; }

        public virtual int SomeInt { get; set; }

        public virtual SuperCause SuperCause { get; set; }

        //public override string ToString()
        //{
        //    return ChildName;
        //}
    }
}