using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;
using System.Text;

namespace WebsiteTemplate.Controllers
{
    public class DeflateCompressionAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuted(ActionExecutedContext actContext)
        {
            base.OnActionExecuted(actContext);
            return;

            var body = actContext.HttpContext.Response.Body;
            //if (actContext.HttpContext.Response.Body.Length == 0)
            //{
            //    base.OnActionExecuted(actContext);
            //    return;
            //}
            byte[] content;
            using (var reader = new BinaryReader(actContext.HttpContext.Response.Body, Encoding.UTF8))//, true, 1024, true))
            {
                content = reader.ReadBytes((int)actContext.HttpContext.Response.Body.Length);
                actContext.HttpContext.Response.Body.Position = 0;
            }
            //var content = actContext.HttpContext.Response.Body;
            if (content == null)
            {
                System.Console.WriteLine("Why");
                base.OnActionExecuted(actContext);
                return;
            }
            var originalType = content == null ? "application/json" : actContext.HttpContext.Response.ContentType;
            
            var bytes = content == null ? null : content;

            actContext.HttpContext.Response.Headers.Remove("Content-Type");

            //TODO: We're losing original headers, need to copy them.
            foreach (var item in actContext.HttpContext.Response.Headers)
            {
                if (item.Key == "Content-Type")
                {
                    continue;
                }
                //else if (item.Key == "ETag")
                //{
                //    actContext.Response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue(item.Value.ToString());
                //}
                else
                {
                    actContext.HttpContext.Response.Headers.Add(item.Key, item.Value);
                }
                
            }
            actContext.HttpContext.Response.Headers.Add("Content-encoding", "deflate");
            actContext.HttpContext.Response.ContentType = originalType;
            actContext.HttpContext.Response.Headers.Add("Content-Type", originalType);

            var zlibbedContent = bytes == null ? new byte[0] : CompressionHelper.DeflateByte(bytes);
            actContext.HttpContext.Response.Body.WriteAsync(zlibbedContent).AsTask().Wait();// = new ByteArrayContent(zlibbedContent);

            base.OnActionExecuted(actContext);
        }
    }
}