using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QBic.Core.Services;
using QBic.Core.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public sealed class GoogleBackupService : BackgroundService
    {
        private readonly ILogger<GoogleBackupService> Logger;
        private readonly IApplicationSettings AppSettings;
        private readonly DataService DataService;
        private readonly BackupService BackupService;

        public GoogleBackupService(ILogger<GoogleBackupService> logger, IApplicationSettings appSettings, DataService dataService, BackupService backupService)
        {
            Logger = logger;
            AppSettings = appSettings;
            DataService = dataService;
            BackupService = backupService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await RunBackgroundService(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in Google backup service: " + ex.Message);
            }
        }

        private async Task RunBackgroundService(CancellationToken stoppingToken)
        {
            await DelayUntilNextUtcTime(AppSettings.GoogleBackupConfig.DailyRunTimeUTC);

            // first get the run time
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunBackup();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // wait until the next day to run it again
            }
        }

        private async Task DelayUntilNextUtcTime(TimeOnly targetTimeUtc)
        {
            // Get the current UTC time
            var nowUtc = DateTime.UtcNow;
            var currentTimeUtc = TimeOnly.FromDateTime(nowUtc);

            // Determine the next occurrence of the target time
            DateTime nextOccurrence;
            if (currentTimeUtc < targetTimeUtc)
            {
                // If the target time is later today
                nextOccurrence = nowUtc.Date.Add(targetTimeUtc.ToTimeSpan());
            }
            else
            {
                // If the target time has already passed today, wait until tomorrow
                nextOccurrence = nowUtc.Date.AddDays(1).Add(targetTimeUtc.ToTimeSpan());
            }

            // Calculate the delay time in milliseconds
            var delay = nextOccurrence - nowUtc;

            // Use Task.Delay to wait for the calculated duration
            await Task.Delay(delay);
        }

        // https://quintonn.github.io/blog/#!/entry/general/august_2024_google_backups
        private async Task RunBackup()
        {
            using var session = DataService.OpenSession();
            var config = session.QueryOver<GoogleBackupSettings>().List().FirstOrDefault();
            if (config == null)
            {
                Logger.LogWarning("No google backup config found in database, not performing google backup");
                return;
            }

            Logger.LogInformation(config.ApplicationName);

            var googleService = await GetGoogleService(config);

            var fileName = "Backup_" + DateTime.Now.ToString("dd_MM_yyyy-HH:mm:ss") + ".db";

            var fileDescription = "Backup File";
            var fileMime = "application/x-sqlite3";


            var backupData = BackupService.CreateFullBackup();
            var mem = new MemoryStream(backupData);

            var driveFile = new Google.Apis.Drive.v3.Data.File();
            driveFile.Name = fileName;
            driveFile.Description = fileDescription;
            driveFile.MimeType = fileMime;
            driveFile.Parents = new string[] { config.ParentFolder };


            var request = googleService.Files.Create(driveFile, mem, fileMime);
            request.Fields = "id";
            // 
            var response = request.Upload();
            if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                Logger.LogError(response.Exception, "Error while uploading backup to google folder: " + response.Exception.Message);
            }
            else
            {
                Logger.LogInformation("Backup saved to google drive with id: " + request.ResponseBody.Id);
            }
        }

        private async Task<DriveService> GetGoogleService(GoogleBackupSettings config)
        {
            var scopes = new[] { DriveService.Scope.DriveFile };

            // Load the service account credentials from the JSON key file
            GoogleCredential credential;
            using (var stream = new MemoryStream(config.CredentialJsonValue))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.DriveFile);
            }

            // Create the Drive API service
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = config.ApplicationName,
            });
            return service;
        }
    }
}
