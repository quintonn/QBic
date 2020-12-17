using QBic.Core.Models;
using QBic.Core.Utilities;
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
using System.IO.Compression;

namespace WebsiteTemplate.Test.MenuItems
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
            
            if (actionNumber != 0)
            {
                result.Add(new CancelInputDialog());
                return result;
            }

            var file = GetValue<WebsiteTemplate.Menus.InputItems.FileInfo>("File");
            var connectionString = ConfigurationManager.ConnectionStrings["MainDataStore"]?.ConnectionString;

            var bytes = file.Data;
            var base64 = QBicUtils.GetString(bytes);
            base64 = base64.Replace("data:;base64,", "").Replace("\0", "");
            bytes = Convert.FromBase64String(base64);

            bytes = CompressionHelper.InflateByte(bytes, CompressionLevel.Optimal);

            if (connectionString.Contains("##CurrentDirectory##"))
            {
                var currentDirectory = Environment.CurrentDirectory;// HttpRuntime.AppDomainAppPath;
                var filePath = currentDirectory + "\\Data\\appData_test.db";
                File.WriteAllBytes(filePath, bytes);
                
                result.Add(new ShowMessage("Done"));
            }
            else
            {
                var json = QBicUtils.GetString(bytes);

                json = "[" + json + "]";
                json = json.Replace("}{", "},{");

                var itemsList = JsonHelper.DeserializeObject<List<BaseClass>[]>(json, true);
                var items = itemsList.SelectMany(i => i).ToList();
                Console.WriteLine(items.Count());
            }

            result.Add(new ShowMessage("Fail"));

            return result;
        }
    }
}