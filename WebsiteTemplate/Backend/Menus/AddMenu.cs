using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class AddMenu : ModifyMenu
    {
        public AddMenu(MenuService menuService, MenuProcessor menuProcessor, DataService dataService)
            : base(menuProcessor, true, menuService, dataService)
        {

        }

        public override EventNumber GetId()
        {
            return EventNumber.AddMenu;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }
    }
}