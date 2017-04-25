using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Menus.ViewItems.CoreItems
{
    public abstract class CoreAction : DoSomething
    {
        protected DataService DataService { get; set; }

        public CoreAction(DataService dataService)
        {
            DataService = dataService;
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
    }
}