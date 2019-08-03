using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.CsvUpload
{
    public class ShowCsvProcessResult : OpenFile
    {
        public override bool AllowInMenu => false;

        public override string Description => "Show CSV Upload Results";

        private string _FileName { get; set; }

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var json = JsonHelper.Parse(data);
            var filePath = json.GetValue("filePath");
            var fileName = json.GetValue("fileName");
            var extension = json.GetValue("extension");
            var mimeType = json.GetValue("mimeType");

            var result = new FileInfo();

            result.FileName = "Bulk Invite Results";
            result.FileExtension = "txt";
            result.Data = System.IO.File.ReadAllBytes(filePath);
            result.MimeType = "text/plain";

            _FileName = result.GetFullFileName();

            return result;
        }

        public override string GetFileNameAndExtension()
        {
            return _FileName;
        }

        public override EventNumber GetId()
        {
            return EventNumber.ShowCsvProcessResult;
        }
    }
}