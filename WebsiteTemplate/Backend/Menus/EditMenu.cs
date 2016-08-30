using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class EditMenu : ModifyMenu
    {
        public EditMenu(MenuService service)
            : base(service, false)
        {

        }

        public override EventNumber GetId()
        {
            return EventNumber.EditMenu;
        }
    }
}