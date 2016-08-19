using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebsiteTemplate.Controllers
{
    public class FileActionResult : IHttpActionResult
    {
        public FileActionResult(Menus.InputItems.FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        private Menus.InputItems.FileInfo FileInfo { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(FileInfo.MimeType))
            {
                throw new ArgumentNullException("MimeType", "FileInfo.MimeType cannot be empty. This is returned by types of OpenFile");
            }
            var base64 = String.Empty;
            if (FileInfo.Data == null || FileInfo.Data.Length == 0)
            {
                //throw new ArgumentNullException("FileInfo.Data", "FileInfo.Data cannot be null or empty. This is returned by types of OpenFile");
            }
            else
            {
                base64 = Convert.ToBase64String(FileInfo.Data);
            }
            var response = new HttpResponseMessage();
            response.Content = new StringContent(base64);
            
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");   /// Tells the browser to try and display the file instead of downloading it.

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(FileInfo.MimeType);

            response.Headers.Add("FileName", FileInfo.GetFullFileName());

            return Task.FromResult(response);
        }
    }
}