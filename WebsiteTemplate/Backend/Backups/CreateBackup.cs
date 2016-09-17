using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Backups
{
    public class CreateBackup : OpenFile
    {
        private BackupService BackupService { get; set; }

        public CreateBackup(BackupService backupService)
        {
            BackupService = backupService;
        }

        public override string Description
        {
            get
            {
                return "Create system backup";
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        //TODO: Need to ask user for confirmation. Maybe even have file name as input

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var result = new FileInfo();

            result.Data = BackupService.CreateBackupOfAllData();

            //var tmpBytes = Controllers.CompressionHelper.InflateByte(result.Data);
            //var tmpString = Utilities.XXXUtils.GetString(tmpBytes);

            //var items = Utilities.JsonHelper.DeserializeObject<List<BaseClass>>(tmpString, true);

            result.FileExtension = "dat";
            result.FileName = "Backup-" + DateTime.UtcNow.ToString("dd-MM-yyyy");
            result.MimeType = "application/octet-stream";  //"application/zip"

            return result;
        }

        public override EventNumber GetId()
        {
            return EventNumber.CreateBackup;
        }
    }
}