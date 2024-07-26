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

        // TODO:
        // todo. add auto backups option in the settings tab
        //       https://medium.com/@meghnav274/uploading-files-to-google-drive-using-net-console-app-f0aae69a3f0f
        //       this will allow users to set a google drive ID of a folder they want to have auto backups placed into
        //       I can add a setting for how many backups to keep
        //       and i can store the back ups (google drive ID) made in a DB too, so that I can delete the cloud backups once we exceed to configured backup count.
        //       this will allow me to let people choose to have backups saved in google drive, or i can have it backed-up into my own google drive folder
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

        //TODO:
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