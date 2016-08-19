using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    public class FileInfo
    {
        public byte[] Data { get; set; }

        public string FileName { get; set; }

        public string FileExtension { get; set; }
       
        public string MimeType { get; set; }

        public int Size
        {
            get
            {
                return Data == null ? 0 : Data.Length;
            }
        }

        public FileInfo()
        {

        }

        public FileInfo(JsonHelper json)
        {
            if (json == null || String.IsNullOrWhiteSpace(json.ToString()))
            {
                return;
            }
            var data = json.GetValue("Data");
            Data = Convert.FromBase64String(data);
            FileName = json.GetValue("FileName");
            MimeType = json.GetValue("MimeType");
            var index = FileName.IndexOf(".");
            FileExtension = json.GetValue("FileExtension");
            
            if (index > -1)
            {
                FileName = FileName.Substring(0, index);
            }
        }

        public string GetFullFileName()
        {
            var fileName = FileName;
            if (!String.IsNullOrWhiteSpace(FileExtension))
            {
                if (!FileExtension.Contains("."))
                {
                    fileName += ".";
                }
                fileName += FileExtension;
            }

            return fileName;
        }
    }
}