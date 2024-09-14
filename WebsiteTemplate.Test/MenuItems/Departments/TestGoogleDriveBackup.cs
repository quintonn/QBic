using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using QBic.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments
{
    public class TestGoogleDriveBackup : DoSomething
    {
        private readonly BackupService BackupService;
        public TestGoogleDriveBackup(DataService dataService, BackupService backupService) : base(dataService)
        {
            BackupService = backupService;
            // https://medium.com/@tanmays.archesoftronix/upload-files-on-g-drive-programmatically-7c3d315d7fee
        }

        public override bool AllowInMenu => false;

        public override string Description => "Test Google Drive Backup";

        public override EventNumber GetId()
        {
            return MenuNumber.TestGoogleDriveBackup;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            await UploadFile();
            return new List<IEvent>()
            {
                new ShowMessage("test done")
            };
        }

        // TODO: Create my own blog entry for this and create inputs on settings screen for these things
        // https://medium.com/@tanmays.archesoftronix/upload-files-on-g-drive-programmatically-7c3d315d7fee
        private async Task<DriveService> GetService()
        {
            var applicationName = "Qbic-Test";

            var scopes = new[]  { DriveService.Scope.DriveFile };

            var jsonFile = "C:\\Users\\Quintonn\\Downloads\\qbic-test-c33fad81c986.json";
            // Load the service account credentials from the JSON key file
            GoogleCredential credential;
            using (var stream = new FileStream(jsonFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential .FromStream(stream).CreateScoped(DriveService.Scope.DriveFile);
            }

            // Create the Drive API service
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
            return service;
        }

        public async Task<string> UploadFile()
        {
            var service = await GetService();

            var fileName = "Backup_" + DateTime.Now.ToString("dd_MM_yyyy-HH:mm:ss") + ".db"; ;

            var parentFolder = "1OfE5__cuU3kt4Ibj4Yv6D5Fr6mEYPf2T";
            
            var fileDescription = "Backup File";
            var fileMime = "application/x-sqlite3";
            

            var backupData = BackupService.CreateFullBackup();
            var mem = new MemoryStream(backupData);

            var driveFile = new Google.Apis.Drive.v3.Data.File();
            driveFile.Name = fileName;
            driveFile.Description = fileDescription;
            driveFile.MimeType = fileMime;
            driveFile.Parents = new string[] { parentFolder };


            var request = service.Files.Create(driveFile, mem, fileMime);
            request.Fields = "id";
            // 
            var response = request.Upload();
            if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw response.Exception;
            }

            return request.ResponseBody.Id;
        }
    }
}