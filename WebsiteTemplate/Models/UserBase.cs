﻿using QBic.Authentication;
using QBic.Core.Models;

namespace WebsiteTemplate.Models
{
    public abstract class UserBase : BaseClass, IUser
    {
        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual UserStatus UserStatus { get; set; }

        public UserBase()
        {
            UserStatus = UserStatus.Active;
        }

        public UserBase(bool canDelete)
            :base()
        {
            CanDelete = canDelete;
        }
    }
}