namespace QBic.Core.Auth
{
    public abstract class AuthConfig<AUTH_RESOLVER> : IAuthConfig where AUTH_RESOLVER : class, IAuthResolver
    {
        public abstract AuthType AuthType { get; }
    }
}
