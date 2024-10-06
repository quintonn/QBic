namespace QBic.Core.Auth
{
    public class OidcAuth<AUTH_RESOLVER> : AuthConfig<AUTH_RESOLVER>, IOidcAuth  where AUTH_RESOLVER :  class, IAuthResolver 
    {
        public override AuthType AuthType => AuthType.Oidc;
        public string ClientId { get; set; }
        public string Authority { get; set; }
        public string RedirectUrl { get; set; }
        public string Scope { get; set; }
        public string ResponseType { get; set; }
    }
}
