using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Mappings
{
    public class UserRoleMap : ClassMap<UserRole>
    {
        public UserRoleMap()
        {
            Id(x => x.Id).GeneratedBy.Increment();
                
            Map(x => x.Description)
                .Not.Nullable();
            Map(x => x.Name)
                .Not.Nullable();
        }
    }
}
