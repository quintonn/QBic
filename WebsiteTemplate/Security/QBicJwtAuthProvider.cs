using QBic.Authentication;
using System;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Security
{
    public class QBicJwtAuthProvider : JwtAuthenticationProvider<IRefreshToken, IUser>
    {
        private ApplicationSettingsCore AppSettings { get; set; }
        public QBicJwtAuthProvider(IServiceProvider serviceProvider, ApplicationSettingsCore appSettings) : base(serviceProvider)
        {
            AppSettings = appSettings;
        }

        public override bool AllowInsecureHttp => false; // this can be false, even behind reverse proxy if UseForwardedHeaders is setup correctly

        public override string Path => AppSettings.TokenEndpointPath;

        public override string ClientId => AppSettings.ClientId;

        public override string Issuer => AppSettings.ClientId;

        public override string Audience => AppSettings.ClientId;

        public override string SecretKey => AppSettings.ApplicationPassPhrase;
    }
}
