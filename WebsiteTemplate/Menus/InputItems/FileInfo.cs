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

        public string FileExtension
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(FileName) && FileName.Contains("."))
                {
                    return FileName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
                }
                return String.Empty;
            }
        }

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
            var jsonString = jtoken.ToString();
            if (String.IsNullOrWhiteSpace(jsonString))
            {
                return;
            }
            var json = JObject.Parse(jsonString);
            var data = json.GetValue("Data").ToString();
            Data = Convert.FromBase64String(data);
            //var fileItem = json.GetValue("FileItem") as JObject;
            FileName = json.GetValue("FileName").ToString();
            MimeType = json.GetValue("MimeType").ToString();
        }
    }
}