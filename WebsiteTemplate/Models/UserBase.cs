using BasicAuthentication.Core.Users;
using Benoni.Core.Models;

namespace WebsiteTemplate.Models
{
    public abstract class UserBase : BaseClass, ICoreIdentityUser
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