using QCumber.Core.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
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
                var connection = ConfigurationManager.ConnectionStrings["MainDataStore"];
                var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
                
                var success = false;
                try
                {
                    var typesToIgnore = new List<Type>();
                    if (restoreSystemSettings == false)
                    {
                        typesToIgnore.Add(typeof(Models.SystemSettings));
                        typesToIgnore.Add(typeof(Models.SystemSettingValue));
                    }
                    BackupService.BusyWithBackups = true;
                    //BackupService.RemoveExistingData(mainConnectionString, typesToIgnore.ToArray());
                    success = BackupService.RestoreFullBackup(true, backupFile.Data, mainConnectionString, typesToIgnore.ToArray());
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