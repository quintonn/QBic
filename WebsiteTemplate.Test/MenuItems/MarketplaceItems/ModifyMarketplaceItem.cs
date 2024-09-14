using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using QBic.Authentication;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.MarketplaceItems
{
    public abstract class ModifyMarketplaceItem : CoreModify<MarketplaceItem>
    {
        private UserManager<IUser> UserManager { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }
        public ModifyMarketplaceItem(DataService dataService, bool isNew, UserManager<IUser> userManager, IHttpContextAccessor httpContextAccessor) : base(dataService, isNew)
        {
            HttpContextAccessor = httpContextAccessor;
            UserManager = userManager;
        }

        public override string EntityName => "Marketplace Item";

        public override EventNumber GetViewNumber()
        {
            return MenuNumber.ViewMarketplaceItems;
        }

        public override List<InputField> InputFields()
        {
            var result = new List<InputField>();

            result.Add(new StringInput("Name", "Name", Item?.Name, null, true));
            result.Add(new StringInput("Description", "Description", Item?.Description, null, true));
            result.Add(new StringInput("Details", "Details", Item?.Details, null, false)
            {
                MultiLineText = true,
            });

            var userTask = QBicUtils.GetLoggedInUserAsync(UserManager, HttpContextAccessor);
            userTask.Wait();
            var currentUser = userTask.Result as User;

            if (IsNew || currentUser.Id == Item.Owner?.Id)
            {
                result.Add(new FileInput("File", "File", null, false));
            }

            result.Add(new HiddenInput("Owner", currentUser?.Id));

            return result;
        }

        public override async Task<IList<IEvent>> PerformModify(bool isNew, string id, NHibernate.ISession session)
        {
            MarketplaceItem dbItem;
            if (isNew)
            {
                dbItem = new MarketplaceItem();
                dbItem.Owner = session.Get<User>(GetValue("Owner"));
            }
            else
            {
                dbItem = session.Get<MarketplaceItem>(id);
            }

            dbItem.Name = GetValue("Name");
            dbItem.Description = GetValue("Description");
            dbItem.Details = GetValue("Details");

            var fileInfo = GetValue<FileInfo>("File");

            dbItem.LastUpdate = DateTime.Now;

            DataService.SaveOrUpdate(session, dbItem);

            if (fileInfo != null && fileInfo.Data != null && fileInfo.Data.Length > 0)
            {
                // check if there is an existing item, and delete it
                var existingFiles = session.QueryOver<FileItem>().Where(x => x.MarketplaceItem.Id == dbItem.Id).List().ToList();
                foreach (var item in existingFiles)
                {
                    session.Delete(item);
                }

                var dbFile = new FileItem();
                dbFile.MarketplaceItem = dbItem;
                dbFile.FileName = fileInfo.FileName;
                dbFile.FileExtension = fileInfo.FileExtension;
                dbFile.MimeType = fileInfo.MimeType;
                dbFile.FileData = fileInfo.Data;
                DataService.SaveOrUpdate(session, dbFile);
            }

            return null;
        }
    }
}
