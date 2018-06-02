using Unity;
using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Utilities
{
    public abstract class ApplicationStartup
    {
        protected DataService DataService { get; set; }

        public ApplicationStartup(DataService dataService)
        {
            DataService = dataService;
        }

        public abstract void RegisterUnityContainers(IUnityContainer container);

        public abstract void SetupDefaults();

        internal void SetupDefaultsInternal()
        {
            //XXXUtils.SetCurrentUser("System");
            SetupDefaults();
        }
    }
}