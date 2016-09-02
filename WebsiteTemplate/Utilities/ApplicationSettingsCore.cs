using Microsoft.Practices.Unity;
using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Utilities
{
    public abstract class ApplicationSettingsCore
    {
        protected DataService DataService { get; set; }

        public ApplicationSettingsCore(DataService dataService)
        {
            DataService = dataService;
        }
        public abstract string GetApplicationName();

        public abstract void RegisterUnityContainers(IUnityContainer container);

        public abstract void SetupDefaults();

        internal void SetupDefaultsInternal()
        {
            DataService.PerformAuditing = false;
            //TODO: Not sure if disabling auditing is what i want to do though????
            try
            {
                SetupDefaults();
            }
            
            finally
            {
                DataService.PerformAuditing = true;
            }
        }
    }
}