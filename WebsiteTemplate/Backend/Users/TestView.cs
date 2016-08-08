using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

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

        public override int GetId()
        {
            return EventNumber.Test;
        }

        public override WebsiteTemplate.Menus.InputItems.FileInfo GetFileInfo(string data)
        {
            var json = Newtonsoft.Json.Linq.JObject.Parse(data);
            var id = json.GetValue("Id").ToString(); 
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