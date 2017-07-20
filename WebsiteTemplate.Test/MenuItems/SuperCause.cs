using System;
using System.Collections.Generic;
using WebsiteTemplate.Data.BaseTypes;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Test.MenuItems
{
    public class SuperCause : DynamicClass
    {
        public virtual string SuperName { get; set; }

        public virtual LongString LongName { get; set; }

        public virtual bool SuperOk { get; set; }

        public virtual DateTime? SuperDate { get; set; }

        public virtual int CauseNumber { get; set; }

        public virtual ISet<CauseChild> CauseChildren { get; set; }

        public virtual byte[] FileData { get; set; }

        //public virtual CauseChild SuperChild { get; set; }

        public SuperCause()
        {
            CauseChildren = new HashSet<CauseChild>();
        }
    }
}