using System.IO;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestLargeFile : OpenFile
    {
        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override string Description
        {
            get
            {
                return "Test Opening Large File";
            }
        }

        public override async Task<WebsiteTemplate.Menus.InputItems.FileInfo> GetFileInfo(string data)
        {
            var result = new WebsiteTemplate.Menus.InputItems.FileInfo();

            var path = @"D:\Projects\ClaimManager\ClaimManager\Temp\backup_Test.bak";
            result.Data = File.ReadAllBytes(path);
            result.FileExtension = "bak";
            result.FileName = "backup_test";
            result.MimeType = "application/octet-stream";  //"application/zip"

            return result;
        }

        public override string GetFileNameAndExtension()
        {
            return "backup_test.bak";
        }

        public override EventNumber GetId()
        {
            return 8382;
        }
    }
}