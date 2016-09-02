using System.Net.Http;
using System.Web.Http.Filters;
using System.Linq;
using System.Collections.Generic;

namespace WebsiteTemplate.Controllers
{
    public class DeflateCompressionAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuted(HttpActionExecutedContext actContext)
        {
            var content = actContext.Response?.Content;
            if (content == null)
            {
                System.Console.WriteLine("Why");
            }
            var originalType = content == null ? new List<string>() { "application/json" } : content.Headers.GetValues("Content-Type");
            var bytes = content == null ? null : content.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] :
            CompressionHelper.DeflateByte(bytes);
            actContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actContext.Response.Content.Headers.Remove("Content-Type");
            actContext.Response.Content.Headers.Add("Content-encoding", "deflate");
            
            actContext.Response.Content.Headers.Add("Content-Type", originalType?.FirstOrDefault());
            base.OnActionExecuted(actContext);
        }
    }
}