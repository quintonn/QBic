namespace QBic.Core.Auth
{
    public class QbicAuth : AuthConfig<QbicAuthResolver>
    {
        public override AuthType AuthType => AuthType.Qbic;
    }
}
