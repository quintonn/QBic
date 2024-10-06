namespace QBic.Core.Auth
{
    public interface IOidcAuth
    {
        public string ClientId { get; set; }
        public string Authority { get; set; }
        public string RedirectUrl { get; set; }
        public string Scope { get; set; }
        public string ResponseType { get; set; }
    }
}
