using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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
            var response = new HttpResponseMessage();

            if (FileInfo.Data == null || FileInfo.Data.Length == 0)
            {
                throw new ArgumentNullException("FileInfo.Data", "FileInfo.Data cannot be null or empty. This is returned by types of OpenFile");
            }
            else if (FileInfo.MimeType != "application/octet-stream")
            {
                //var base64 = Convert.ToBase64String(FileInfo.Data); //data too big
                //response.Content = new StringContent(base64);
                response.Content = new ByteArrayContent(FileInfo.Data);
            }
            else // zipped content
            {
                response.Content = new PushStreamContent(async (stream, content, context) =>
                {
                    try
                    {
                        var bufferSize = 65536;
                        var length = FileInfo.Size;
                        var bytesDone = 0;

                        while (length > 0)
                        {
                            var bytesToSend = Math.Min(bufferSize, FileInfo.Size - bytesDone);
                            await stream.WriteAsync(FileInfo.Data, bytesDone, bytesToSend);
                            length -= bytesToSend;
                            bytesDone += bytesToSend;
                        }
                    }
                    catch (Exception ee)
                    {
                        //Console.WriteLine(ee.Message);
                        throw;
                    }
                    finally
                    {
                        stream.Close();
                    }
                });
            }

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline") /// Tells the browser to try and display the file instead of downloading it.
            {
                FileName = FileInfo.GetFullFileName()
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(FileInfo.MimeType);

            response.Headers.Add("FileName", FileInfo.GetFullFileName());

            return Task.FromResult(response);
        }
    }
}