using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestReadBackup : GetInput
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
                return "Test Reading Backup";
            }
        }

        public override EventNumber GetId()
        {
            return new EventNumber(9375);
        }

        public override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();
            result.Add(new FileInput("File", "File"));
            return result;
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            var result = new List<IEvent>();

            var file = GetValue<WebsiteTemplate.Menus.InputItems.FileInfo>("File");
            var connectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;
            if (connectionString.Contains("##CurrentDirectory##"))
            {
                var bytes = file.Data;

                var base64 = XXXUtils.GetString(bytes);
                base64 = base64.Replace("data:;base64,", "");
                bytes = Convert.FromBase64String(base64);

                string decodedString = Encoding.UTF8.GetString(bytes);
                bytes = Convert.FromBase64String(decodedString);

                try
                {
                    bytes = CompressionHelper.InflateByte(bytes, Ionic.Zlib.CompressionLevel.BestCompression);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                //bytes = CompressionHelper.InflateByte(bytes);

                File.WriteAllBytes(@"D:\Projects\WebsiteTemplate\WebsiteTemplate\Data\restore.db", bytes);
                result.Add(new ShowMessage("Done"));
            }
            else
            {
                var bytes = file.Data;

                var base64 = XXXUtils.GetString(bytes);
                base64 = base64.Replace("data:;base64,", "");
                //bytes = Convert.FromBase64String(base64);

                bytes = XXXUtils.GetBytes(base64);
                string decodedString = Encoding.UTF8.GetString(bytes);

                //base64 = base64.Replace('+', '-').Replace('/', '_');
                base64 = base64.Replace("\0", "");
                bytes = Convert.FromBase64String(base64);

                try
                {
                    bytes = CompressionHelper.InflateByte(bytes, Ionic.Zlib.CompressionLevel.BestCompression);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                var json = XXXUtils.GetString(bytes);

                json = "[" + json + "]";
                var jsonSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                json = json.Replace("}{", "},{");
                //var tmp = JsonConvert.DeserializeObject(json, jsonSettings);
                
                var itemsList = JsonHelper.DeserializeObject<List<BaseClass>[]>(json, true);
                var items = itemsList.SelectMany(i => i).ToList();
                Console.WriteLine(items.Count());
            }

            result.Add(new ShowMessage("Fail"));

            return result;
        }
    }
}