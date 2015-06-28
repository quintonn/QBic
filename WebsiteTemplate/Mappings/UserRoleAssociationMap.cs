using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Mappings
{
    public class UserRoleAssociationMap : BaseClassMap<UserRoleAssociation>
    {
        public UserRoleAssociationMap()
        {
            Table("UserRoleAssociations");

            References(x => x.User).Column("IdUser")
                           .Not.Nullable()
                           .LazyLoad(Laziness.False);

            Map(x => x.UserRole)
              .CustomType<UserRole>()
              .Not.Nullable();
        }
    }
}
