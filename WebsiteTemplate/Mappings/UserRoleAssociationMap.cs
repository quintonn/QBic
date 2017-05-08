using FluentNHibernate.Mapping;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class UserRoleAssociationMap : BaseClassMap<UserRoleAssociation>
    {
        public UserRoleAssociationMap()
        {
            Table("UserRoleAssociation");

            References(x => x.User).Column("IdUser")
                           .Not.Nullable()
                           .NotFound.Ignore()
                           .LazyLoad(Laziness.False);

            References(x => x.UserRole).Column("IdUserRole")
                           .Not.Nullable()
                           .NotFound.Ignore()
                           .LazyLoad(Laziness.False);
            //Map(x => x.UserRole)
            //  .CustomType<UserRoleEnum>()
            //  .Not.Nullable();
        }
    }
}