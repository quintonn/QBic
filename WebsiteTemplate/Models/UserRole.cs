using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public class UserRole : BaseClass
    {
        public virtual string Name { get; set; }

        public virtual string Description { get; set; }
    }
}