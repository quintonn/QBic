using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class BackupProcessor : CoreProcessor<byte[]>
    {
        public BackupProcessor(IUnityContainer container)
            : base(container)
        {
            BackupService = container.Resolve<BackupService>();
            AppSettings = container.Resolve<ApplicationSettingsCore>();
        }

        private BackupService BackupService { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }

        public override async Task<byte[]> ProcessEvent(int eventId)
        {
            //var originalData = GetRequestData();
            
            //var jsonString = Encryption.Decrypt(originalData, AppSettings.ApplicationPassPhrase);
            //var json = JsonHelper.Parse(jsonString);
            
            //Todo: decrypt data and check for a certain value to confirm this request is legit

            var result = BackupService.CreateBackupOfAllData();

            return result;
        }
    }
}