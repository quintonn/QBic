using QBic.Authentication;
using System;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.SiteSpecific
{
    public class MobileJwtAuthProvider : JwtAuthenticationProvider<IRefreshToken, MobileUser>
    {
        public MobileJwtAuthProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string Path => "/mobile/token";

        public override bool AllowInsecureHttp => true;

        public override string ClientId => "PapyrusApp";

        public override string Issuer => "PapyrusApp";

        public override string Audience => "PapyrusApp";

        public override TimeSpan AccessTokenExpiration => TimeSpan.FromHours(6);

        public override TimeSpan RefreshTokenExpiration => TimeSpan.FromDays(30);

        public override string SecretKey => "Super Secret Key";
    }
}
