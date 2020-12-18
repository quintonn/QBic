using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QBic.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    [Route("api/v1/menu")]
    public class MenuController : ControllerBase
    {
        //private DataStore Store { get; set; }
        private DataService DataService { get; set; }

        private UserService UserService { get; set; }
        private UserManager<IUser> UserManager { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        public MenuController(DataService dataService, UserService userService, UserManager<IUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            DataService = dataService;
            UserService = userService;
            UserManager = userManager;
            HttpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUrl()
        {
            //var request = Request.GetRequestContext();
            var uri = Request.Path;// request.Url.Request.RequestUri;
            var result = Request.Scheme + "://" + Request.Host + Request.PathBase;
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RequestPasswordReset")]
        [Microsoft.AspNetCore.Mvc.RequireHttps]
        public async Task<IActionResult> RequestPasswordReset()
        {
            //XXXUtils.SetCurrentUser("System");

            var data = await GetRequestDataAsync();
            var json = JsonHelper.Parse(data);
            var usernameOrEmail = json.GetValue("usernameOrEmail");

            try
            {
                var result = await UserService.SendPasswordResetLink(usernameOrEmail);

                return new JsonResult(result);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail")]
        [Microsoft.AspNetCore.Mvc.RequireHttps]
        public async Task<IActionResult> ConfirmEmail()
        {
            try
            {
                // Set current user to 'System' user for auditing purposes. Because no user will be logged in at the moment.
                //XXXUtils.SetCurrentUser("System");

                var queryString = this.Request.Query;// .GetQueryNameValuePairs();
                var userId = queryString.Single(q => q.Key == "userId").Value;
                var emailToken = queryString.Single(q => q.Key == "token").Value;

                IdentityResult verifyToken;
                using (var session = DataService.OpenSession())
                {
                    var dbUser = session.Get<User>(userId);
                    verifyToken = await UserManager.ConfirmEmailAsync(dbUser, emailToken);
                }
                
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

        protected async Task<string> GetRequestDataAsync()
        {
            return await WebsiteUtils.GetCurrentRequestData(HttpContextAccessor);
        }
    }
}

/*
Next -> 
     -> Also a forgot username/password button  --> Or maybe not.
     -> Add a way to prevent certain information from being deleted (eg, admin user).*/