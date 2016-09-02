using System.IO;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Users
{
    public class TestView : OpenFile
    {
        public override string Description
        {
            get
            {
                return "Test";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.Test;
        }

        public override WebsiteTemplate.Menus.InputItems.FileInfo GetFileInfo(string data)
        {
            var json = JsonHelper.Parse(data);
            var id = json.GetValue("Id"); 
            var result = new WebsiteTemplate.Menus.InputItems.FileInfo();
            //result.Data = File.ReadAllBytes(@"D:\Quintonn\Documents\Unisa Degree.pdf");
            result.Data = File.ReadAllBytes(@"D:\Quintonn\Documents\quintonn-rothmann-cv-1st-revision.docx");
            result.FileName = "CV.docx";
            //result.MimeType = "application/pdf";
            result.MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            return result;
        }
    }
}