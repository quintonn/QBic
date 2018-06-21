using QBic.Core.Services;
using System;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

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
                return "Create Backup";
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

            result.Data = BackupService.CreateFullBackup();

            result.FileExtension = "dat";
            result.FileName = "Backup-" + DateTime.UtcNow.ToString("dd-MM-yyyy");
            result.MimeType = "application/octet-stream";  //"application/zip"

            return result;
        }

        public override string GetFileNameAndExtension()
        {
            return "Backup-" + DateTime.UtcNow.ToString("dd-MM-yyyy") + ".dat";
        }

        public override EventNumber GetId()
        {
            return EventNumber.CreateBackup;
        }
    }
}