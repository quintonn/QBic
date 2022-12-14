using NHibernate;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class DeleteMarketplaceItem : CoreDeleteAction<MarketplaceItem>
    {
        public DeleteMarketplaceItem(DataService dataService) : base(dataService)
        {
        }

        public override string EntityName => "Marketplace Item";

        public override EventNumber ViewNumber => MenuNumber.ViewMarketplaceItems;

        public override EventNumber GetId()
        {
            return MenuNumber.DeleteMarketplaceItem;
        }

        public override void DeleteOtherItems(ISession session, MarketplaceItem mainItem)
        {
            var dbFiles = session.QueryOver<FileItem>().Where(f => f.MarketplaceItem.Id == mainItem.Id).List().ToList();
            foreach (var item in dbFiles)
            {
                DataService.TryDelete(session, item);
            }
            base.DeleteOtherItems(session, mainItem);
        }
    }
}
