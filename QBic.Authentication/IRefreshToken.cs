namespace QBic.Authentication
{
    public interface IRefreshToken
    {
        string Id { get; }

        string Token { get; set; }
    }
}
