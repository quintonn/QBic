﻿using FluentNHibernate.Mapping;
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

            References(x => x.UserRole).Column("IdUserRole")
                           .Not.Nullable()
                           .LazyLoad(Laziness.False);
            //Map(x => x.UserRole)
            //  .CustomType<UserRoleEnum>()
            //  .Not.Nullable();
        }
    }
}