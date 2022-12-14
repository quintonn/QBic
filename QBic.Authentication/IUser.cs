namespace QBic.Authentication
{
    public interface IUser
    {
        string Id { get; }

        string UserName { get; set; }

        string PasswordHash { get; set; }

        string Email { get; set; }

        bool EmailConfirmed { get; set; }
    }
}
