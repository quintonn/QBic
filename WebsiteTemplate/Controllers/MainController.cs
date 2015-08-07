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
using System.Threading.Tasks;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.SiteSpecific.Utilities;
using WebsiteTemplate.Menus.BaseItems;
using System.Net.Http;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.SiteSpecific.UIActionItems;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        public static IDictionary<int, UIAction> UIActionList { get; set; }

        private DataStore Store { get; set; }

        public MainController()
        {
            Store = new DataStore();
        }

        static MainController()
        {
            CheckDefaultValues();
            PopulateUIActionList();
        }

        private static void PopulateUIActionList()
        {
            UIActionList = new Dictionary<int, UIAction>();

            var types = typeof(UIAction).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(UIAction)))
                {
                    var instance = (UIAction)Activator.CreateInstance(type);
                    UIActionList.Add(instance.Id, instance);
                }
            }
        }

        private static void CheckDefaultValues()
        {
            var store = new DataStore();
            try
            {
                using (var session = store.OpenSession())
                {
                    var adminUser = session.CreateCriteria<User>()
                                               .Add(Restrictions.Eq("UserName", "Admin"))
                                               .UniqueResult<User>();
                    if (adminUser == null)
                    {
                        adminUser = new User(false)
                        {
                            Email = "q10athome@gmail.com",
                            EmailConfirmed = true,
                            UserName = "Admin",
                        };
                        var result = CoreAuthenticationEngine.UserManager.CreateAsync(adminUser, "password");
                        result.Wait();
                        if (!result.Result.Succeeded)
                        {
                            throw new Exception("Unable to create user: " + "Admin");
                            //return result.Result.ToString();
                        }
                    }

                    var viewUsersRoleAssociation = session.CreateCriteria<UserRoleAssociation>()
                                                      .CreateAlias("User", "user")
                                                      .Add(Restrictions.Eq("user.Id", adminUser.Id))
                                                      .Add(Restrictions.Eq("UserRole", UserRole.ViewUsers))
                                                      .UniqueResult<UserRoleAssociation>();
                    if (viewUsersRoleAssociation == null)
                    {
                        viewUsersRoleAssociation = new UserRoleAssociation(false)
                        {
                            User = adminUser,
                            UserRole = UserRole.ViewUsers,
                        };
                        session.Save(viewUsersRoleAssociation);
                    }

                    session.Flush();
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
                //Role = user.UserRole.Name,
                Role = "Admin",
                Id = user.Id
            };
            //var json = JsonConvert.SerializeObject(user);
            return Ok(json);
        }

        [HttpPost]
        [Route("executeUIAction/{*id}")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> ExecuteUIAction(int id)
        {
            var user = await this.GetLoggedInUserAsync();
            var data = await Request.Content.ReadAsStringAsync();

            //var temp = HttpUtility.UrlDecode(data);
            //var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(temp);

            if (!UIActionList.ContainsKey(id))
            {
                return BadRequest("No action has been found for UIActionId: " + id);
            }

            var result = new List<UIAction>();

            var uiAction = UIActionList[id];
            uiAction.Store = Store;
            uiAction.Request = Request;

            if (uiAction is ShowView)
            {
                var action = uiAction as ShowView;
                
                using (var session = Store.OpenSession())
                {
                    var listType = typeof(List<>).MakeGenericType(action.DataType);
                    var myList = Activator.CreateInstance(listType);
                    var list = myList as System.Collections.IList;

                    session.CreateCriteria(action.DataType)
                           //.Add(Restrictions.Eq("", ""))   //TODO: Can add filter/query items here
                           .List(list);
                    action.ViewData = list;
                    result.Add(action);
                }
            }
            else if (uiAction is DoSomething)
            {
                var doResult = await (uiAction as DoSomething).ProcessAction(data);
                result.AddRange(doResult);
            }
            else if (uiAction is GetInput)
            {
                var inputResult = uiAction;
                result.Add(inputResult);
            }
            else if (uiAction is CancelInputDialog)
            {
                return Ok();
            }
            else
            {
                return BadRequest("ERROR: Unknown UIActionType: " + uiAction.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("getUserMenu")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> GetUserMenu()
        {
            var user = await this.GetLoggedInUserAsync();
            //var results = new List<UIAction>();
            var results = new Dictionary<int, string>();
            using (var session = Store.OpenSession())
            {
                var roles = session.CreateCriteria<UserRoleAssociation>()
                                   .CreateAlias("User", "user")
                                   .Add(Restrictions.Eq("user.Id", user.Id))
                                   .List<UserRoleAssociation>()
                                   .ToList();

                roles.ForEach(r =>
                    {
                        var menuIds = GetMenuItemsForAllRoles().Where(i => i.Key == r.UserRole).Select(i => i.Value).ToList();

                        menuIds.ForEach(i =>
                        {
                            if (!UIActionList.ContainsKey(i))
                            {
                                throw new NotImplementedException("No UIAction exists for id " + i);
                            }
                            var uiAction = UIActionList[i];
                            //results.Add(uiAction);
                            results.Add(uiAction.Id, uiAction.MenuLabel);
                        });
                    });
            }
            return Ok(results);
        }

        private Dictionary<UserRole, int> GetMenuItemsForAllRoles()
        {
            var results = new Dictionary<UserRole, int>();

            // So here i can create menus for users based on their roles.
            // So if a user is in 2 roles, he will get all of those menu items.
            //results.Add("User Role", UIActionId);

            //EG
            results.Add(UserRole.ViewUsers, UIActionNumbers.VIEW_USERS);

            return results;
        }

    }
}