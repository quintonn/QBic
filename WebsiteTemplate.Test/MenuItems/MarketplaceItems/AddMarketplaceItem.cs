using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using QBic.Authentication;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class AddMarketplaceItem : ModifyMarketplaceItem
    {
        public AddMarketplaceItem(DataService dataService, UserManager<IUser> userManager, IHttpContextAccessor httpContextAccessor) : base(dataService, true, userManager, httpContextAccessor)
        {
        }

        public override EventNumber GetId()
        {
            return MenuNumber.AddMarketplaceItem;
        }
    }
}
