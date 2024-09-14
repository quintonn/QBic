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

        public static MenuNumber ViewDepartments = new MenuNumber(2100);
        public static MenuNumber AddDepartment = new MenuNumber(2101);
        public static MenuNumber EditDepartment = new MenuNumber(2102);
        public static MenuNumber DeleteDepartment = new MenuNumber(2103);
        public static MenuNumber DepartmentDetailsSection = new MenuNumber(2104);
        public static MenuNumber ExpenseDetailsComponent = new MenuNumber(2105);
        public static MenuNumber TestGoogleDriveBackup = new MenuNumber(2106);

        public static MenuNumber ViewExpenses = new MenuNumber(2110);
        public static MenuNumber AddExpense = new MenuNumber(2111);
        public static MenuNumber EditExpense = new MenuNumber(2112);
        public static MenuNumber DeleteExpense = new MenuNumber(2113);
    }
}