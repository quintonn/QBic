using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Mappings;

namespace WebsiteTemplate.Models
{
    public class SystemSettings : DynamicClass
    {
        public virtual string EmailFromAddress { get; set; }

        public virtual string EmailHost { get; set; }

        public virtual string EmailUserName { get; set; }

        public virtual string EmailPassword { get; set; }

        public virtual int EmailPort { get; set; }

        public virtual bool EmailEnableSsl { get; set; }

        public virtual string DateFormat { get; set; }

        public virtual string WebsiteBaseUrl { get; set; }
    }
}