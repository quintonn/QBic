namespace WebsiteTemplate.Models
{
    public class User : UserBase
    {
        public User()
            : base()
        {
        }
        public User(bool canDelete)
            : base(canDelete)
        {
        }
    }
}