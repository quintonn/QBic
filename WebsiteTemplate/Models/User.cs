using BasicAuthentication.Users;

namespace WebsiteTemplate.Models
{
    public class User : BaseClass, ICoreIdentityUser
    {
        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual UserStatus UserStatus { get; set; }

        public User()
        {
            UserStatus = Models.UserStatus.Active;
        }

        public User(bool canDelete)
            :base()
        {
            CanDelete = canDelete;
        }
    }
}