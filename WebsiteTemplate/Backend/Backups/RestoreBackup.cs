using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.Backups
{
    public class RestoreBackup : GetInput
    {
        private BackupService BackupService { get; set; }

        public RestoreBackup(BackupService backupService)
        {
            BackupService = backupService;
        }
        public override string Description
        {
            get
            {
                return "Restore Backup";
            }
        }

        public override IList<InputField> GetInputFields()
        {
            var results = new List<InputField>();
            results.Add(new FileInput("BackupFile", "Backup File", mandatory: true));
            results.Add(new BooleanInput("SystemSettings", "Restore System Settings", false));
            return results;
        }

        public override EventNumber GetId()
        {
            return EventNumber.RestoreBackup;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog()
                };
            }
            else if (actionNumber == 0)
            {
                var backupFile = GetValue<FileInfo>("BackupFile");
                var restoreSystemSettings = GetValue<bool>("SystemSettings");

                var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
                var providerName = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ProviderName;
                var success = false;
                try
                {
                    BackupService.BusyWithBackups = true;
                    BackupService.RemoveExistingData(mainConnectionString, restoreSystemSettings);
                    success = BackupService.RestoreFullBackup(backupFile.Data, mainConnectionString, providerName, restoreSystemSettings);
                }
                finally
                {
                    BackupService.BusyWithBackups = false;
                }

                return new List<IEvent>()
                {
                    new ShowMessage(success ? "Backup restored successfully." : "Backup unsuccessful")
                };
            }
            return new List<IEvent>();
        }
    }
}