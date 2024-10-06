using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class AddMarketplaceItem : ModifyMarketplaceItem
    {
        public AddMarketplaceItem(DataService dataService, ContextService contextService) : base(dataService, true, contextService)
        {
        }

        public override EventNumber GetId()
        {
            return MenuNumber.AddMarketplaceItem;
        }
    }
}
