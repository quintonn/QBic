using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public FileInfo(JToken jtoken)
        {
            var json = jtoken as JObject;
            if (json == null)
            {
                return;
            }
            var data = json.GetValue("Data").ToString();
            Data = Convert.FromBase64String(data);
            FileName = json.GetValue("FileName").ToString();
            MimeType = json.GetValue("MimeType").ToString();
            var index = FileName.IndexOf(".");
            FileExtension = json.GetValue("FileExtension")?.ToString();
            
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