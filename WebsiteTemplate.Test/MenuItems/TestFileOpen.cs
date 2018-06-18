using QCumber.Core.Utilities;
using System.Text;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestFileOpen : OpenFile
    {
        private static string DELIMITER = ";";
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override string Description
        {
            get
            {
                return "Test file open";
            }
        }
        private string _fileName { get; set; }

        public override async Task<FileInfo> GetFileInfo(string data)
        {
            var result = new FileInfo();

            result.FileName = "Active Users Report";
            result.FileExtension = "csv";
            result.Data = CreateCsvFile(data);
            result.MimeType = "text/csv";

            _fileName = result.GetFullFileName();
            return result;
        }
        private byte[] CreateCsvFile(string info)
        {
            var data = new StringBuilder();
            data.AppendLine("TEST FILE DOWNLOAD");
            data.AppendFormat("User{0}News Read{0}New Releases Read", DELIMITER);
            data.AppendLine();
            data.Append(info);
            data.AppendLine();

            var result = QCumberUtils.GetBytes(data.ToString());
            return result;
        }

        public override string GetFileNameAndExtension()
        {
            return _fileName;
        }

        public override EventNumber GetId()
        {
            return 741;
        }
    }
}