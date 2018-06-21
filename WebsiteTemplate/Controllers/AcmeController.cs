using System.Web.Http;
using System.Linq;
using System.Net.Http;
using System.Net;
using WebsiteTemplate.Utilities;
using System.IO;
using System;
using log4net;
using QBic.Core.Utilities;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix(".well-known/acme-challenge")]
    public class AcmeController : ApiController
    {
        private static readonly ILog Logger = SystemLogger.GetLogger<AcmeController>();

        public static string ChallengeResponse { get; set; } = "xxx";
        public static string ChallengePath { get; set; } = "--";

        [HttpGet]
        [Route("{*path}")]
        [AllowAnonymous]
        public IHttpActionResult Test(string path)
        {
            //TODO: Not sure if I even need this. Maybe the other request can set the file in the correct path.
            //      No! Do it this way, can clear challenge path and response after request is made

            var request = System.Web.HttpContext.Current.Request.Url.ToString();
            var challenge = request.Split("/".ToCharArray()).Last();

            Logger.Info("Received ACME challenge on path: " + path + " and challenge = " + challenge);
            
            //Todo: Challenge should = path also.
            //      But, I should also validate/verify both the path and the request. Could be a hijacker/MiM

            if (challenge != ChallengePath)
            {
                //return BadRequest(); TODO: Put this in and test.
            }

            var resp = challenge + "." + ChallengeResponse;
            if (ChallengeResponse == "xxx") // Not sure if i need this. Why would this code run like this and not be set?? 
            {
                var dir = QBicUtils.GetCurrentDirectory();
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

            //TODO: Not sure if this is called only once or twice per challenge.
            //      But need to allow below code

            //ChallengePath = String.Empty;
            //ChallengeResponse = String.Empty;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(resp)
            };

            return ResponseMessage(response);
        }
    }
}