using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Mappings;

namespace WebsiteTemplate.Models
{
    public class UserRole : DynamicClass
    {
        public virtual string Name { get; set; }

        public virtual string Description { get; set; }
    }
}