using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
                
                var stringContent = await resp.Content.ReadAsStringAsync();
                stringContent = stringContent.Substring(1, stringContent.Length - 2);
                
                var data = Convert.FromBase64String(stringContent);
                var responseData = CompressionHelper.InflateByte(data);
                
                var itemsString = XXXUtils.GetString(responseData);

                var items = JsonHelper.DeserializeObject<List<BaseClass>>(itemsString, true);
                Console.WriteLine(items.Count);
            }

            return new List<IEvent>()
            {
                new ShowMessage("Test done")
            };
        }
    }
}