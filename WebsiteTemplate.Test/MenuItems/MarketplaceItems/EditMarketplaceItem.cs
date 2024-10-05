using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class EditMarketplaceItem : ModifyMarketplaceItem
    {
        public EditMarketplaceItem(DataService dataService, ContextService contextService) : base(dataService, false, contextService)
        {
        }

        public override EventNumber GetId()
        {
            return MenuNumber.EditMarketplaceItem;
        }
    }
}
