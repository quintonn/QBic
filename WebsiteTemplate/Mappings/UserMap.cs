using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class UserMap : BaseClassMap<User>
    {
        public UserMap()
        {
            Table("Users");

            Map(x => x.UserName)
              .Not.Nullable();
            Map(x => x.Email)
                .Not.Nullable(); ;
            Map(x => x.EmailConfirmed);
            Map(x => x.PasswordHash)
                .Not.Nullable();
        }
    }
}
