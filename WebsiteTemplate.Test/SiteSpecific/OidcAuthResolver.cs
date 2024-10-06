using Microsoft.AspNetCore.Http;
using QBic.Authentication;
using QBic.Core.Auth;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.SiteSpecific
{
    public class OidcAuthResolver : IAuthResolver
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly DataService DataService;
        private readonly IOidcAuth AuthConfig;

        public OidcAuthResolver(IHttpContextAccessor httpContextAccessor, DataService dataService, ApplicationSettingsCore appSettings)
        {
            HttpContextAccessor = httpContextAccessor;
            DataService = dataService;
            AuthConfig = appSettings.AuthConfig as IOidcAuth;
        }

        public async Task<IUser> GetUser(ClaimsPrincipal principal)
        {
            if (string.IsNullOrWhiteSpace(principal?.Identity?.Name))
            {
                return null;
            }

            // Use the current request's auth header to call the OIDC user info endpoint
            var request = HttpContextAccessor?.HttpContext?.Request;
            var headers = request?.Headers;
            if (headers != null && headers.ContainsKey("Authorization"))
            {
                var authToken = headers["Authorization"].Where(x => x.StartsWith("Bearer")).FirstOrDefault()?.Replace("Bearer ", "");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                
                var url = new Uri(new Uri(AuthConfig.Authority), "/oidc/v1/userinfo").ToString();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonHelper.Parse(content);

                    using var session = DataService.OpenSession();
                    var existingUser = session.Get<User>(principal.Identity.Name);
                    if (existingUser == null)
                    {
                        existingUser = new User()
                        {
                            Email = json.GetValue("email"),
                            EmailConfirmed = json.GetValue<bool>("email_verified"),
                            Id = principal.Identity.Name,
                            UserName = json.GetValue("given_name"),
                            PasswordHash = "",
                            UserStatus = UserStatus.Active
                        };
                        session.SaveOrUpdate(existingUser);
                    }

                    // Read the user's assigned roles to map them to Qbic roles
                    var approvedRoles = json.GetValue<JsonHelper>("urn:zitadel:iam:org:project:roles");
                    if (approvedRoles != null)
                    {
                        // TODO: This is site specific and will require domain specific knowledge
                        //       The following is an example to look for and create the Admin role for the user
                        
                        var adminRole = approvedRoles.GetValue("admin");  // An 'admin' role exists in the OIDC provider
                        if (adminRole != null)
                        {
                            var temp = adminRole.ToString();

                            // we map the 'admin' role to the "Admin" role in QBic/This App
                            var dbRole = session.QueryOver<UserRole>().Where(x => x.Name == "Admin").SingleOrDefault();
                            if (dbRole != null)
                            {
                                var userId = existingUser.Id;
                                var existingUserRoleAssociation = session.QueryOver<UserRoleAssociation>()
                                                                         .Where(x => x.User.Id == userId && x.UserRole.Id == dbRole.Id)
                                                                         .SingleOrDefault();
                                if (existingUserRoleAssociation == null)
                                {
                                    existingUserRoleAssociation = new UserRoleAssociation()
                                    {
                                        User = existingUser,
                                        UserRole = dbRole
                                    };
                                    session.SaveOrUpdate(existingUserRoleAssociation);
                                }
                            }
                        }
                    }

                    session.Flush();

                    return existingUser;
                }
            }

            
            return null;
        }
    }
}
