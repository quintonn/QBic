using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Mappings;

namespace WebsiteTemplate.Backend.TestItem
{
    public class SuperCause : DynamicClass
    {
        public virtual string SuperName { get; set; }

        public virtual bool SuperOk { get; set; }
    }
}