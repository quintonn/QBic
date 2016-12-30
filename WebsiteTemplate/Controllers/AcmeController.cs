using System.Web.Http;
using System.Linq;
using System.Net.Http;
using System.Net;

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

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(challenge + "." + ChallengeResponse)
            };

            return ResponseMessage(response);
        }
    }
}