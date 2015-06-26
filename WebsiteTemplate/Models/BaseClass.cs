using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public abstract class BaseClass
    {
        public virtual string Id { get; set; }

        public virtual bool CanDelete { get; set; }

        public BaseClass()
        {
            CanDelete = true;
        }
    }
}