using Qactus.Authorization.Core;
using Qactus.Authorization.Jwt.Default;
using System;

namespace WebsiteTemplate.Test
{
    public class TestJwtAuthProvider : JwtAuthenticationProvider<IRefreshToken, IUser>
    {
        public TestJwtAuthProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string Path => "/api/v1/token";

        public override string ClientId => "QBic";

        public override string Issuer => "QBic";

        public override string Audience => "QBic";

        public override string SecretKey => "my test password";
    }
}
