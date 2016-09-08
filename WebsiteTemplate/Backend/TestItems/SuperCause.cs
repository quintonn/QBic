using System;
using System.Collections.Generic;
using WebsiteTemplate.Mappings;

namespace WebsiteTemplate.Backend.TestItems
{
    public class SuperCause : DynamicClass
    {
        public virtual string SuperName { get; set; }

        public virtual bool SuperOk { get; set; }

        public virtual DateTime? SuperDate { get; set; }

        public virtual IList<CauseChild> CauseChildren { get; set; }
    }
}