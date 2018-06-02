using BasicAuthentication.Security;
using BasicAuthentication.Users;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.DefaultsForTest;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1/menu")]
    public class MenuController : ApiController
    {
        //private DataStore Store { get; set; }
        private DataService DataService { get; set; }

        private UserService UserService { get; set; }
        private DefaultUserManager UserManager { get; set; }

        public MenuController(DataService dataService, UserService userService, DefaultUserManager userManager)
        {
            DataService = dataService;
            UserService = userService;
            UserManager = userManager;
        }

        private string GetCurrentUrl()
        {
            var request = Request.GetRequestContext();
            var uri = request.Url.Request.RequestUri;
            var result = uri.Scheme + "://" + uri.Host + request.VirtualPathRoot;
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RequestPasswordReset")]
        [RequireHttps]
        public async Task<IHttpActionResult> RequestPasswordReset()
        {
            //XXXUtils.SetCurrentUser("System");

            var data = GetRequestData();
            var json = JsonHelper.Parse(data);
            var usernameOrEmail = json.GetValue("usernameOrEmail");

            try
            {
                var result = await UserService.SendPasswordResetLink(usernameOrEmail);

                return Json(result);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail")]
        [RequireHttps]
        public async Task<IHttpActionResult> ConfirmEmail()
        {
            try
            {
                // Set current user to 'System' user for auditing purposes. Because no user will be logged in at the moment.
                //XXXUtils.SetCurrentUser("System");

                var queryString = this.Request.GetQueryNameValuePairs();
                var userId = queryString.Single(q => q.Key == "userId").Value;
                var emailToken = queryString.Single(q => q.Key == "token").Value;
                var verifyToken = await UserManager.ConfirmEmailAsync(userId, emailToken);
                
                if (verifyToken.Succeeded)
                {
                    /// Maybe show a confirmation/welcome page
                    using (var session = DataService.OpenSession())
                    {
                        var user = session.Load<User>(userId);
                        var url = GetCurrentUrl() + "?confirmed=" + HttpUtility.UrlEncode(user.UserName);
                        return Redirect(url);
                    }
                }
                else
                {
                    var message = String.Join("\n", verifyToken.Errors);
                    //return BadRequest(message);
                    //This won't work but is just an example of what to do
                    //return Redirect("https://localhost/CustomIdentity/Pages/Error.html?Errors=" + verifyToken.Result.Errors.First());
                    return Redirect(GetCurrentUrl() + "?errors=" + HttpUtility.UrlEncode(message));
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        protected string GetRequestData()
        {
            using (var stream = HttpContext.Current.Request.InputStream)
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                var res = System.Text.Encoding.UTF8.GetString(mem.ToArray());
                return res;
            }
        }
    }
}

/*
Next -> 
     -> Also a forgot username/password button  --> Or maybe not.
     -> Add a way to prevent certain information from being deleted (eg, admin user).*/