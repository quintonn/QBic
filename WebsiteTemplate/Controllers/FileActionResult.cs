using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebsiteTemplate.Controllers
{
    public class FileActionResult : IActionResult
    {
        public FileActionResult(Menus.InputItems.FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        private Menus.InputItems.FileInfo FileInfo { get; set; }

        public Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("Content-Disposition", new ContentDispositionHeaderValue("inline") /// Tells the browser to try and display the file instead of downloading it.
            {
                FileName = FileInfo.GetFullFileName()
            }.ToString());

            context.HttpContext.Response.Headers.Add("Content-Type", FileInfo.MimeType);
            context.HttpContext.Response.Headers.Add("FileName", FileInfo.GetFullFileName());

            if (String.IsNullOrWhiteSpace(FileInfo.MimeType))
            {
                throw new ArgumentNullException("MimeType", "FileInfo.MimeType cannot be empty. This is returned by types of OpenFile");
            }
            
            if (FileInfo.Data == null || FileInfo.Data.Length == 0)
            {
                throw new ArgumentNullException("FileInfo.Data", "FileInfo.Data cannot be null or empty. This is returned by types of OpenFile");
            }
            else if (FileInfo.MimeType != "application/octet-stream")
            {
                //var base64 = Convert.ToBase64String(FileInfo.Data); //data too big
                //response.Content = new StringContent(base64);
                context.HttpContext.Response.Body.Write(FileInfo.Data);// = new ByteArrayContent(FileInfo.Data);
            }
            else // zipped content
            {
                using (var msi = new MemoryStream(FileInfo.Data))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        msi.CopyTo(gs);
                    }

                    context.HttpContext.Response.Headers.Add("Content-Encoding", "gzip");
                    context.HttpContext.Response.Body.WriteAsync(mso.ToArray());
                }
                //response.Content = new PushStreamContent(async (stream, content, context) =>
                //{
                //    try
                //    {
                //        var bufferSize = 65536;
                //        var length = FileInfo.Size;
                //        var bytesDone = 0;

                //        while (length > 0)
                //        {
                //            var bytesToSend = Math.Min(bufferSize, FileInfo.Size - bytesDone);
                //            await stream.WriteAsync(FileInfo.Data, bytesDone, bytesToSend);
                //            length -= bytesToSend;
                //            bytesDone += bytesToSend;
                //        }
                //    }
                //    catch (Exception ee)
                //    {
                //        //Console.WriteLine(ee.Message);
                //        throw;
                //    }
                //    finally
                //    {
                //        stream.Close();
                //    }
                //});
            }
            
            return Task.FromResult(0);
        }

       
    }
}