using NHibernate;
using QBic.Core.Utilities;
using System.Text;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.CsvDownload
{
    public abstract class CsvFileDownloader : OpenFile
    {
        private DataService DataService { get; set; }
        public CsvFileDownloader(DataService dataService)
        {
            DataService = dataService;
        }

        public override bool AllowInMenu => false;

        protected virtual string DELIMITER { get; } = ",";

        private string _fileName { get; set; }
        private StringBuilder ReportData { get; set; }

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var result = new FileInfo();
            ReportData = new StringBuilder();

            result.FileName = Description;
            result.FileExtension = "csv";
            result.Data = CreateCsvFile(data);
            result.MimeType = "text/csv";

            _fileName = result.GetFullFileName();
            return result;
        }

        public override string GetFileNameAndExtension()
        {
            return _fileName;
        }

        protected void AddItems(params string[] values)
        {
            foreach (var value in values)
            {
                ReportData.Append($"\"{value}\"{DELIMITER}");
            }
        }

        protected void AddItemsLine(params string[] values)
        {
            foreach (var value in values)
            {
                ReportData.AppendLine($"\"{value}\"{DELIMITER}");
            }
        }

        protected abstract void ProcessCsvData(string data, ISession session);

        private byte[] CreateCsvFile(string data)
        {
            using (var session = DataService.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                ProcessCsvData(data, session);

                transaction.Commit();
            }

            var result = QBicUtils.GetBytes(ReportData.ToString());
            return result;
        }
    }
}