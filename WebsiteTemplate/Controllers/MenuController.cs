using BasicAuthentication.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebsiteTemplate.Models;
using BasicAuthentication.ControllerHelpers;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1/menu")]
    public class MenuController : ApiController
    {
        [HttpGet]
        [Route("test")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public IHttpActionResult Test()
        {
            //System.Threading.Thread.Sleep(60000);
            return Ok("successfull menu test");
        }
    }
}