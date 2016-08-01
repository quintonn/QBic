using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix(".well-known/acme-challenge")]
    public class AcmeController : ApiController
    {
        [HttpGet]
        [Route("{*path}")]
        [AllowAnonymous]
        public IHttpActionResult Test(string path)
        {
            //TODO: Can create class and input screen to manage this
            return Json("successs: " + path);
        }
    }
}