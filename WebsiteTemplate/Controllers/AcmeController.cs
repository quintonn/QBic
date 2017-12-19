using System.Web.Http;
using System.Linq;
using System.Net.Http;
using System.Net;
using WebsiteTemplate.Utilities;
using System.IO;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix(".well-known/acme-challenge")]
    public class AcmeController : ApiController
    {
        public static string ChallengeResponse { get; set; } = "xxx";

        [HttpGet]
        [Route("{*path}")]
        [AllowAnonymous]
        public IHttpActionResult Test(string path)
        {
            var request = System.Web.HttpContext.Current.Request.Url.ToString();
            var challenge = request.Split("/".ToCharArray()).Last();

            var resp = challenge + "." + ChallengeResponse;
            if (ChallengeResponse == "xxx")
            {
                var dir = XXXUtils.GetCurrentDirectory();
                var physicallPath = dir + "\\.well-known\\acme-challenge\\" + challenge;
                if (File.Exists(physicallPath))
                {
                    resp = File.ReadAllText(physicallPath);
                }
                else
                {
                    resp = string.Format("Path {0} does not exist", physicallPath);
                }
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(resp)
            };

            return ResponseMessage(response);
        }
    }
}