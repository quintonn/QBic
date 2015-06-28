using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Models
{
    public class UserRoleAssociation : BaseClass
    {
        public virtual User User { get; set; }

        public virtual UserRole UserRole { get; set; }

        public UserRoleAssociation()
            : base()
        {

        }

        public UserRoleAssociation(bool canDelete)
            : base(canDelete)
        {

        }
    }
}