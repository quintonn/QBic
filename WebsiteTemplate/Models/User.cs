using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public class User : ICoreIdentityUser
    {
        public virtual string Id { get; set; }

        public virtual int IntId
        {
            get
            {
                return Convert.ToInt32(Id);
            }
            set
            {
                Id = value.ToString();
            }
        }

        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual UserRole UserRole { get; set;  }

        public virtual UserStatus UserStatus { get; set; }

        public User()
        {
            UserStatus = Models.UserStatus.Active;
        }
    }
}