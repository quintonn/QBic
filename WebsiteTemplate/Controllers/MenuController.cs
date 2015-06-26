using BasicAuthentication.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebsiteTemplate.Models;
using BasicAuthentication.ControllerHelpers;
using WebsiteTemplate.Data;
using Newtonsoft.Json;
using System.Transactions;
using BasicAuthentication.Users;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Configuration;
using System.Diagnostics;
using WebsiteTemplate.SiteSpecific.Utilities;
using System.Net.Mail;
using System.Net.Http;

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

        static MenuController()
        {
            //CheckDefaultValues();
        }

        [HttpGet]
        [Route("getUsers")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public IHttpActionResult GetUsers()
        {
            IList<User> users;
            using (var session = Store.OpenSession())
            {
                users = session.CreateCriteria<User>()
                               .List<User>().ToList();
                session.Flush();
            }
            return Json(users);
        }

        [HttpDelete]
        [Route("deleteUser/{*id}")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(id);
                session.Delete(user);
                session.Flush();
            }
            return Ok();
        }

        [HttpGet]
        [Route("getUserRoles")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public async Task<IHttpActionResult> GetUserRoles()
        {
            using (var session = Store.OpenSession())
            {
                var items = session.CreateCriteria<UserRole>()
                               .List<UserRole>().ToList();
                return Json(items);
            }
        }

        [HttpPost]
        [Route("resendConfirmationEmail/{*id}")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public async Task<IHttpActionResult> resendConfirmationEmail(string id)
        {
            var emailSent = false;
            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(Convert.ToInt32(id));
                emailSent = await SendConfirmationEmail(user.Id, user.UserName, user.Email);
            }
            if (emailSent == true)
            {
                return Ok("Email confirmation resent successfully");
            }

            return BadRequest("Email confirmation could not be sent again. Contact your system administrator.");
        }

        [HttpPost]
        [Route("createUser")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public async Task<IHttpActionResult> CreateUser()
        {
            var data = Request.Content.ReadAsStringAsync();
            data.Wait();
            
            var temp2 = HttpUtility.UrlDecode(data.Result);
            //var temp = JsonConvert.DeserializeObject(temp2)

            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(temp2);

            //var parameters = this.ParseFormData(temp2);

            var name = parameters["name"];
            var email = parameters["email"];
            var password = parameters["password"];
            var confirmPassword = parameters["confirmPassword"];
            var userRoleId = Convert.ToInt32(parameters["userRoleId"]);

            if (confirmPassword != password)
            {
                return BadRequest("Password and password confirmation do not match");
            }

            UserRole userRole;
            using (var session = Store.OpenSession())
            {
                userRole = session.Get<UserRole>(userRoleId);
            }

            if (userRole == null)
            {
                return BadRequest("No user role found with id: " + userRoleId);
            }

            var user = new User()
            {
                Email = email,
                UserName = name,
                UserRole = userRole
            };
            var result = await CoreAuthenticationEngine.UserManager.CreateAsync(user, password);
            if (result.Succeeded == false)
            {
                var message = String.Join("\n", result.Errors);
                return BadRequest(message);
            }
            else
            {
                var emailSent = await SendConfirmationEmail(user.Id, user.UserName, user.Email);
                if (emailSent == false)
                {
                    return Ok("User created, but was unable to send activation email");
                }
            }

            return Ok("User created successfully.\nCheck your inbox for confirmation email to activate your account");
        }

        private async Task<bool> SendConfirmationEmail(string userId, string userName, string emailAddress)
        {
            var smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
            if (smtp == null)
            {
                Trace.WriteLine("No system.net/mailSettings/smtp section in web.config or app.config");
                return await Task.FromResult(false);
            }

            var emailToken = CoreAuthenticationEngine.UserManager.GenerateEmailConfirmationTokenAsync(userId).Result;

            var myuri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);

            var body = "Hi " + userName;
            body += "\nWelcome to " + WebsiteTemplateConstants.ApplicatoinName;

            body += "\n\nPlease click on the following link to activate and activate your email:\n";

            body += GetCurrentUrl() + "/api/v1/menu/ConfirmEmail?userId=" + userId + "&token=" + HttpUtility.UrlEncode(emailToken);

            var mailMessage = new MailMessage(smtp.From, emailAddress, "Email Confirmation", body);
            
            var sendEmailTask = Task.Run(() =>
                {
                    try
                    {
                        var smtpClient = new SmtpClient(smtp.Network.Host, smtp.Network.Port);

                        smtpClient.Credentials = new System.Net.NetworkCredential(smtp.Network.UserName, smtp.Network.Password);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.EnableSsl = smtp.Network.EnableSsl;

                        smtpClient.Send(mailMessage);
                    }
                    catch (Exception e)
                    {
                        var message = e.Message + "\n" + e.ToString();
                        Console.WriteLine(message);
                        Trace.WriteLine(message);
                        Debug.WriteLine(message);
                        return false;
                    }
                    return true;
                });
            return await sendEmailTask;
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
                        var user = session.Load<User>(Convert.ToInt32(userId));
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
Next -> Add a re-send confirmation email on view of users
     -> Also a forgot username/password button  --> Or maybe not.
     -> Add a way to prevent certain information from being deleted (eg, admin user).*/