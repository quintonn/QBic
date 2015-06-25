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

        [HttpGet]
        [Route("getUserRoles")]
        [RequireHttps]
        [Authorize]
        [RoleAuthorization("Admin")]
        public IHttpActionResult GetUserRoles()
        {
            using (var session = Store.OpenSession())
            {
                var items = session.CreateCriteria<UserRole>()
                               .List<UserRole>().ToList();
                return Json(items);
            }
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

            return Ok();
        }
    }
}