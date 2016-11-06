using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestBackupProcessor : DoSomething
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
                return "Test Backup Web Call";
            }
        }

        public override EventNumber GetId()
        {
            return 89356;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var url = "https://localhost/websitetemplate/api/v1/performBackup";
            var json = new JsonHelper();
            json.Add("abc", "xyz");
            var bytes = XXXUtils.GetBytes(json.ToString());

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            //using (var client = new WebClient())
            using (var client = new HttpClient())
            {
                //var responseData = client.UploadData(url, bytes);
                //var tmpString = XXXUtils.GetString(responseData);

                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var resp = await client.PostAsJsonAsync(url, bytes);
                var backupTypeString = resp.Headers.GetValues(BackupService.BACKUP_HEADER_KEY).FirstOrDefault();
                var backupType = (BackupType)Enum.Parse(typeof(BackupType), backupTypeString);

                switch (backupType)
                {
                    case BackupType.JsonData:
                        var stringContent = await resp.Content.ReadAsStringAsync();
                        stringContent = stringContent.Substring(1, stringContent.Length - 2);

                        var data = Convert.FromBase64String(stringContent);
                        var responseData = CompressionHelper.InflateByte(data);

                        var itemsString = XXXUtils.GetString(responseData);

                        itemsString = "[" + itemsString + "]";
                        itemsString = itemsString.Replace("}{", "},{");
                        var itemsList = JsonHelper.DeserializeObject<List<BaseClass>[]>(itemsString, true);
                        var items = itemsList.SelectMany(i => i).ToList();
                        Console.WriteLine(items.Count);
                        break;
                    case BackupType.SQLiteFile:
                        //var currentDirectory = HttpRuntime.AppDomainAppPath;
                        //var filePath = currentDirectory + "\\Data\\appData_test.db";
                        //File.WriteAllBytes(filePath, bytes);
                        
                        // Don't do anything, just save the file somewhere
                        break;
                    default:
                        return new List<IEvent>()
                        {
                            new ShowMessage("Unknown backup type detected: " + backupType.ToString())
                        };
                }

                
            }

            return new List<IEvent>()
            {
                new ShowMessage("Test done")
            };
        }
    }
}