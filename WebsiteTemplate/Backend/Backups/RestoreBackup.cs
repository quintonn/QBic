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
            results.Add(new EnumComboBoxInput<BackupType>("BackupType", "Backup Type", false, x => x.Key != BackupType.Unknown, x => x.Value)
            {
                Mandatory = true
            });
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
                var backupType = GetValue<BackupType>("BackupType");

                var mainConnectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;

                if (backupType == BackupType.JsonData)
                {
                    BackupService.RemoveExistingData(mainConnectionString);
                    BackupService.RestoreBackupOfAllData(backupFile.Data, mainConnectionString);
                }
                else if (backupType == BackupType.SqlFullBackup)
                {
                    BackupService.RestoreSqlDatabase(backupFile.Data, mainConnectionString, "WebtestQ", false);
                }
                else if (backupType == BackupType.SQLiteFile)
                {

                }

                return new List<IEvent>()
                {
                    //new CancelInputDialog(),
                    new ShowMessage("Backup restored successfully.")
                };
            }
            return new List<IEvent>();
        }
    }
}