using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class AddMenu : ModifyMenu
    {
        public AddMenu(MenuService menuService)
            :base(menuService, true)
        {
                
        }

        public override EventNumber GetId()
        {
            return EventNumber.AddMenu;
        }
    }
}