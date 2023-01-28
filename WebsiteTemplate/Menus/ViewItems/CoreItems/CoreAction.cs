using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Menus.ViewItems.CoreItems
{
    public abstract class CoreAction : DoSomething
    {
        public CoreAction(DataService dataService) : base(dataService)
        {
            
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