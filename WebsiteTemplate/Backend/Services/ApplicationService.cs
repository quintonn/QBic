using System.Threading.Tasks;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class ApplicationService
    {
        private static ApplicationSettingsCore ApplicationSettings { get; set; }

        public ApplicationService(ApplicationSettingsCore applicationSettings)
        {
            ApplicationSettings = applicationSettings;
        }

        
        public object InitializeApplication()
        {
            var version = XXXUtils.GetApplicationCoreVersion().ToString();

            var json = new
            {
                ApplicationName = ApplicationSettings.GetApplicationName(),
                Version = version
            };

            return json;
        }

        public async Task<object> InitializeSession()
        {
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync() as User;
            var json = new
            {
                User = user.UserName,
                Id = user.Id,
            };
            return json;
        }
    }
}