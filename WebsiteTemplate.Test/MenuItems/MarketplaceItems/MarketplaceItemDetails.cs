using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class MarketplaceItemDetails : CoreAction
    {
        public MarketplaceItemDetails(DataService dataService) : base(dataService)
        {
        }

        public override string Description => "Marketplace Item Details";

        public override EventNumber GetId()
        {
            return MenuNumber.MarketplaceItemDetails;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue("Id");

            string message;

            using (var session = DataService.OpenSession())
            {
                var dbItem = session.Get<MarketplaceItem>(id);

                message = dbItem.Details;
            }

            return new List<IEvent>()
            {
                new ShowMessage(message),
            };
        }
    }
}
