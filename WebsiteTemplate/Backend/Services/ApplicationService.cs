using BasicAuthentication.Users;
using System.Threading.Tasks;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class ApplicationService
    {
        private static ApplicationSettingsCore ApplicationSettings { get; set; }
        private UserContext UserContext { get; set; }

        public ApplicationService(ApplicationSettingsCore applicationSettings, UserContext userContext)
        {
            ApplicationSettings = applicationSettings;
            UserContext = userContext;
        }

        
        public object InitializeApplication(string constructorError)
        {
            //var version = XXXUtils.GetApplicationCoreVersion().ToString();

            var version = ApplicationSettings.GetType().Assembly.GetName().Version.ToString();

            var json = new
            {
                ApplicationName = ApplicationSettings.GetApplicationName(),
                Version = version,
                ConstructorError = constructorError
            };

            return json;
        }

        public async Task<object> InitializeSession()
        {
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync(UserContext) as User;
            if (user == null)
            {
                return new
                {
                    User = "",
                    Id = -1
                };
            }
            var json = new
            {
                User = user.UserName,
                Id = user.Id,
            };
            return json;
        }
    }
}