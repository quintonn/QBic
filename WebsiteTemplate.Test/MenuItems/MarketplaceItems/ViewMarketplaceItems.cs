using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class ViewMarketplaceItems : ShowView
    {
        private DataService DataService { get; set; }
        private readonly ContextService ContextService;
        public ViewMarketplaceItems(DataService dataService, ContextService contextService)
        {
            DataService = dataService;
            ContextService = contextService;
        }

        public override bool AllowInMenu => true;

        public override string Description => "View Marketplace Items";

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Description", "Description");
            columnConfig.AddDateColumn("Last Updated", "LastUpdate");
            columnConfig.AddDateColumn("Owner", "Owner");

            var currentUser = ContextService.GetRequestUser();

            columnConfig.AddLinkColumn("", "Id", "Edit", MenuNumber.EditMarketplaceItem, new ShowHideColumnSetting()
            {
                Conditions = new List<Condition>()
                {
                    new Condition("OwnerId", Comparison.Equals, currentUser?.Id)
                },
                Display = ColumnDisplayType.Show
            });

            columnConfig.AddButtonColumn("", "Id", "Details", MenuNumber.MarketplaceItemDetails);
            columnConfig.AddLinkColumn("", "Id", "Download", MenuNumber.DownloadMarketplaceItem);

            columnConfig.AddButtonColumn("", "Id", "X", new UserConfirmation("Delete Item?", MenuNumber.DeleteMarketplaceItem), new ShowHideColumnSetting()
            {
                Conditions = new System.Collections.Generic.List<Condition>()
                {
                    new Condition("OwnerId", Comparison.Equals, currentUser?.Id)
                },
                Display = ColumnDisplayType.Show
            });
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            using (var session = DataService.OpenSession())
            {
                var data = CreateQuery(session, settings).Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                                                         .Take(settings.LinesPerPage)
                                                         .List<MarketplaceItem>()
                                                         .ToList();
                var result = data.Select(d => new
                {
                    d.Name,
                    d.Description,
                    d.Id,
                    Owner = d.Owner?.UserName,
                    OwnerId = d.Owner?.Id,
                    d.LastUpdate
                }).ToList();
                return result;
            }
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            using (var session = DataService.OpenSession())
            {
                var result = CreateQuery(session, settings).RowCount();
                return result;
            }
        }

        private IQueryOver<MarketplaceItem> CreateQuery(NHibernate.ISession session, GetDataSettings settings)
        {
            var query = session.QueryOver<MarketplaceItem>();

            if (!String.IsNullOrWhiteSpace(settings.Filter))
            {
                query = query.Where(Restrictions.On<MarketplaceItem>(x => x.Name).IsInsensitiveLike(settings.Filter, MatchMode.Anywhere) ||
                                    Restrictions.On<MarketplaceItem>(x => x.Description).IsInsensitiveLike(settings.Filter, MatchMode.Anywhere));
            }

            return query;
        }

        public override EventNumber GetId()
        {
            return MenuNumber.ViewMarketplaceItems;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>()
            { 
                new MenuItem("Add", MenuNumber.AddMarketplaceItem)
            };
        }
    }
}
