using QBic.Core.Models;
using System;

namespace WebsiteTemplate.Test.MenuItems
{
    public class CauseChild : DynamicClass
    {
        public virtual string ChildName { get; set; }

        public virtual int SomeInt { get; set; }

        public virtual SuperCause SuperCause { get; set; }

        public virtual ChildType ChildType { get; set; }

        public virtual DateTime? NullDateTest { get; set; }

        public string ChildTypeDescription
        {
            get
            {
                return ChildType.ToString();
            }
        }

        //public override string ToString()
        //{
        //    return ChildName;
        //}
    }

    public enum ChildType
    {
        Boy,
        Girl
    }
}