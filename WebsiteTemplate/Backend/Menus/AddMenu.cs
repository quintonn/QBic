using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class AddMenu : ModifyMenu
    {
        internal override bool IsNew
        {
            get
            {
                return true;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.AddMenu;
        }

        public AddMenu(MenuService service)
            :base(service)
        {

        }
    }
}