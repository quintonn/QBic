﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QBic.Core.Services;
using System;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class BackupProcessor : CoreProcessor<FileContentResult>
    {
        public BackupProcessor(IServiceProvider container, ILogger<BackupProcessor> logger)
            : base(container, logger)
        {
            BackupService = Container.GetService<BackupService>();
            AppSettings = Container.GetService<ApplicationSettingsCore>();
        }

        private BackupService BackupService { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }

        public override async Task<FileContentResult> ProcessEvent(int eventId)
        {
            var originalData = await GetRequestData();
            var jData = JsonHelper.Parse(originalData);
            //var jsonString = Encryption.Decrypt(originalData, AppSettings.ApplicationPassPhrase);
            //var json = JsonHelper.Parse(jsonString);

            //var requestBackupTypeString = Container.GetService<IHttpContextAccessor>().HttpContext.Request.Headers[BackupService.BACKUP_HEADER_KEY];

            //Todo: decrypt data and check for a certain value to confirm this request is legit

            //var result = BackupService.CreateBackupOfAllData();

            ////System.Web.HttpContext.Current.Response.Headers.Add(BackupService.BACKUP_HEADER_KEY, backupType.ToString());

            //var fileInfo = new FileInfo();
            //fileInfo.Data = result;
            //fileInfo.MimeType = "application/octet-stream";
            var result = new FileInfo();

            result.Data = BackupService.CreateFullBackup();

            result.FileExtension = "dat";
            result.FileName = "Backup-" + DateTime.UtcNow.ToString("dd-MM-yyyy");
            result.MimeType = "application/octet-stream";  //"application/zip"
            return new FileContentResult(result.Data, result.MimeType);
        }
    }
}