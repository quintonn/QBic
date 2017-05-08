using Microsoft.Practices.Unity;
using System;
using System.Configuration;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class BackupProcessor : CoreProcessor<FileActionResult>
    {
        public BackupProcessor(IUnityContainer container)
            : base(container)
        {
            BackupService = container.Resolve<BackupService>();
            AppSettings = container.Resolve<ApplicationSettingsCore>();
        }

        private BackupService BackupService { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }

        public override async Task<FileActionResult> ProcessEvent(int eventId)
        {
            var originalData = GetRequestData();
            var jData = JsonHelper.Parse(originalData);
            //var jsonString = Encryption.Decrypt(originalData, AppSettings.ApplicationPassPhrase);
            //var json = JsonHelper.Parse(jsonString);

            var requestBackupTypeString = System.Web.HttpContext.Current.Request.Headers[BackupService.BACKUP_HEADER_KEY];
            
            //Todo: decrypt data and check for a certain value to confirm this request is legit

            var result = BackupService.CreateBackupOfAllData();

            //System.Web.HttpContext.Current.Response.Headers.Add(BackupService.BACKUP_HEADER_KEY, backupType.ToString());

            var fileInfo = new FileInfo();
            fileInfo.Data = result;
            fileInfo.MimeType = "application/octet-stream";
            return new FileActionResult(fileInfo);
        }
    }
}