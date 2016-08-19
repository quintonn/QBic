using BasicAuthentication.Security;
using BasicAuthentication.Users;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1/menu")]
    public class MenuController : ApiController
    {
        private DataStore Store { get; set; }

        public MenuController()
        {
            Store = new DataStore();
        }

        private string GetCurrentUrl()
        {
            var request = Request.GetRequestContext();
            var uri = request.Url.Request.RequestUri;
            var result = uri.Scheme + "://" + uri.Host + request.VirtualPathRoot;
            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail")]
        [RequireHttps]
        public async Task<IHttpActionResult> ConfirmEmail()
        {
            try
            {
                var queryString = this.Request.GetQueryNameValuePairs();
                var userId = queryString.Single(q => q.Key == "userId").Value;
                var emailToken = queryString.Single(q => q.Key == "token").Value;
                var verifyToken = await CoreAuthenticationEngine.UserManager.ConfirmEmailAsync(userId, emailToken);
                
                if (verifyToken.Succeeded)
                {
                    /// Maybe show a confirmation/welcome page
                    using (var session = Store.OpenSession())
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
    }
}

/*
Next -> 
     -> Also a forgot username/password button  --> Or maybe not.
     -> Add a way to prevent certain information from being deleted (eg, admin user).*/