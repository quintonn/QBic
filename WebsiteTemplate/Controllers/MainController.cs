using BasicAuthentication.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebsiteTemplate.Data;
using BasicAuthentication.ControllerHelpers;
using WebsiteTemplate.Models;
using NHibernate.Criterion;
using BasicAuthentication.Users;
using System.Diagnostics;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        private DataStore Store { get; set; }

        public MainController()
        {
            Store = new DataStore();
        }

        static MainController()
        {
            CheckDefaultValues();
        }

        private static void CheckDefaultValues()
        {
            var store = new DataStore();
            try
            {
                using (var session = store.OpenSession())
                {
                    var adminRole = session.CreateCriteria<UserRole>()
                                           .Add(Restrictions.Eq("Name", "Admin"))
                                           .UniqueResult<UserRole>();
                    if (adminRole == null)
                    {
                        adminRole = new UserRole()
                        {
                            Description = "Administrator",
                            Name = "Admin"
                        };
                        store.Save(adminRole);
                        //session.Save(adminRole);
                        session.Flush();
                    };

                    var adminUser = session.CreateCriteria<User>()
                                               .Add(Restrictions.Eq("UserName", "Admin"))
                                               .UniqueResult<User>();
                    if (adminUser == null)
                    {
                        adminUser = new User()
                        {
                            Email = "Admin@gmail.com",
                            EmailConfirmed = true,
                            UserName = "Admin",
                            UserRole = adminRole,
                            CanDelete = false
                        };
                        var result = CoreAuthenticationEngine.UserManager.CreateAsync(adminUser, "password");
                        result.Wait();
                        if (!result.Result.Succeeded)
                        {
                            throw new Exception("Unable to create user: " + "Admin");
                            //return result.Result.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Trace.WriteLine(e);
                Debug.WriteLine(e);
            }
        }

        [HttpGet]
        [Route("initialize")]
        [RequireHttps]
        [Authorize]
        public IHttpActionResult Initialize()
        {
            var user = this.GetLoggedInUser() as User;
            //var user = GetLoggedInUser();
            var json = new
            {
                User = user.UserName,
                //Email = user.Email,
                Role = user.UserRole.Name,
                Id = user.Id
            };
            //var json = JsonConvert.SerializeObject(user);
            return Ok(json);
        }
    }
}