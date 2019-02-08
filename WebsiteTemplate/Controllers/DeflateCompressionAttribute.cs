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
                base.OnActionExecuted(actContext);
                return;
            }
            var originalType = content == null ? new List<string>() { "application/json" } : content.Headers.GetValues("Content-Type");
            
            var bytes = content == null ? null : content.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] : CompressionHelper.DeflateByte(bytes);
            actContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actContext.Response.Content.Headers.Remove("Content-Type");

            //TODO: We're losing original headers, need to copy them.
            foreach (var item in actContext.Response.Content.Headers)
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
                    actContext.Response.Content.Headers.Add(item.Key, item.Value);
                }
                
            }
            actContext.Response.Content.Headers.Add("Content-encoding", "deflate");
           
            
            actContext.Response.Content.Headers.Add("Content-Type", originalType?.FirstOrDefault());
            base.OnActionExecuted(actContext);
        }
    }
}