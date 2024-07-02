using Newtonsoft.Json;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Test.SiteSpecific
{
    [JsonConverter(typeof(EventNumberConverter))]
    public class MenuNumber : EventNumber
    {
        public MenuNumber(int value)
            : base(value)
        {
        }

        // Use numbers 2000 and above. Numbers below 2000 are for internal menu items.

        public static MenuNumber ViewMarketplaceItems = new MenuNumber(2000);
        public static MenuNumber AddMarketplaceItem = new MenuNumber(2001);
        public static MenuNumber EditMarketplaceItem = new MenuNumber(2002);
        public static MenuNumber DeleteMarketplaceItem = new MenuNumber(2003);
        public static MenuNumber MarketplaceItemDetails = new MenuNumber(2004);
        public static MenuNumber DownloadMarketplaceItem = new MenuNumber(2005);

        public static MenuNumber TestCsvFileUpload = new MenuNumber(2080);
    }
}