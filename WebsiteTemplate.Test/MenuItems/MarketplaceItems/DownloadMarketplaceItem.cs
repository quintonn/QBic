using System;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public class DownloadMarketplaceItem : OpenFile
    {
        public override bool AllowInMenu => false;

        private DataService DataService { get; set; }

        private string _fileName { get; set; }

        public DownloadMarketplaceItem(DataService dataService)
        {
            DataService = dataService;
        }

        public override string Description => "Download Marketplace Item";

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var result = new FileInfo();

            var json = JsonHelper.Parse(data);

            var id = json.GetValue("Id")?.ToString();

            using (var session = DataService.OpenSession())
            {
                var attachment = session.QueryOver<FileItem>().Where(t => t.MarketplaceItem.Id == id).SingleOrDefault();

                result.FileName = attachment.FileName;
                result.FileExtension = attachment.FileExtension;
                result.Data = attachment.FileData;
                result.MimeType = attachment.MimeType;
                if (String.IsNullOrWhiteSpace(result.MimeType))
                {
                    result.MimeType = "text/plain";
                }
            }

            return result;
        }

        public override string GetFileNameAndExtension()
        {
            return _fileName;
        }

        public override EventNumber GetId()
        {
            return MenuNumber.DownloadMarketplaceItem;
        }
    }
}