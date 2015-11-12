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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WebsiteTemplate.SiteSpecific.EventItems;
using WebsiteTemplate.Mappings;
using System.Reflection;
using NHibernate;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        public static IDictionary<EventNumber, Event> EventList { get; set; }

        public static List<string> Log { get; set; }

        private DataStore Store { get; set; }

        public MainController()
        {
            Store = new DataStore();

            //CheckDefaultValues();
        }

        static MainController()
        {
            try
            {
                Log = new List<string>();
                CheckDefaultValues();
                PopulateEventList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void PopulateEventList()
        {
            EventList = new Dictionary<EventNumber, Event>();

            var types = typeof(Event).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Event)))
                {
                    var instance = (Event)Activator.CreateInstance(type);
                    EventList.Add(instance.GetId(), instance);
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

                    var role2 = session.CreateCriteria<UserRoleAssociation>()
                                                      .CreateAlias("User", "user")
                                                      .Add(Restrictions.Eq("user.Id", adminUser.Id))
                                                      .Add(Restrictions.Eq("UserRole", UserRole.ViewUserRoleAssociations))
                                                      .UniqueResult<UserRoleAssociation>();
                    if (role2 == null)
                    {
                        role2 = new UserRoleAssociation(false)
                        {
                            User = adminUser,
                            UserRole = UserRole.ViewUserRoleAssociations
                        };
                        session.Save(role2);
                    }

                    //var menuList1 = session.QueryOver<Menu>()
                    //                   .WhereRestrictionOn(x => x.UserRoleString).IsLike("")
                    //                   .List<Menu>();
                    var menuList1 = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.Eq("Event", EventNumber.ViewUsers))
                                           .List<Menu>();
                    if (menuList1.Count == 0)
                    {
                        var menu1 = new Menu()
                        {
                            Event = EventNumber.ViewUsers,
                            Name = "View Users",
                            AllowedUserRoles = new List<UserRole>() { UserRole.ViewUsers }
                        };

                        session.Save(menu1);
                    }

                    //var menuList2 = session.CreateCriteria<Menu>()
                    //                       .Add(Restrictions.Eq("Event", EventNumber.ViewUserRoleAssociations))
                    //                       .List<Menu>();
                    
                    
                    //if (menuList2.Count == 0)
                    //{
                    //    var menu2 = new Menu()
                    //    {
                    //        Event = EventNumber.ViewUserRoleAssociations,
                    //        Name = "View User Role Associations",
                    //        AllowedUserRoles = new List<UserRole>() { UserRole.ViewUserRoleAssociations }
                    //    };

                    //    session.Save(menu2);
                    //}

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
            return Json(json);
        }

        [HttpPost]
        [Route("processEvent/{*eventId}")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> ProcessEvent(int eventId)
        {
            try
            {
                var data = await Request.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<JObject>(data);
                
                var parameters = json.ToObject<Dictionary<string, object>>();
                var formData = parameters["Data"].ToString();
                var actionId = Convert.ToInt32(parameters["ActionId"]);
                
                var id = (EventNumber)eventId;
                var eventItem = EventList[id] as GetInput;
                var inputButtons = eventItem.InputButtons;
                if (inputButtons.Where(i => i.ActionNumber == actionId).Count() == 0)
                {
                    return Json(new List<Event>()
                    {
                        new ShowMessage("No button with action number " + actionId + " exists for " + eventItem.Description),
                    });
                };
                var result = await eventItem.ProcessAction(formData, actionId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> ExecuteUIAction(int eventId)
        {
            var user = await this.GetLoggedInUserAsync();
            var data = await Request.Content.ReadAsStringAsync();

            //var id = (EventNumber)Enum.Parse(typeof(EventNumber), eventId);
            var id = (EventNumber)eventId;

            //var temp = HttpUtility.UrlDecode(data);
            //var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(temp);

            if (!EventList.ContainsKey(id))
            {
                return BadRequest("No action has been found for event number: " + id);
            }

            var result = new List<Event>();

            var eventItem = EventList[id];
            eventItem.Store = Store;
            eventItem.Request = Request;

            if (eventItem is ShowView)
            {
                var action = eventItem as ShowView;
                
                using (var session = Store.OpenSession())
                {
                    //var listType = typeof(List<>).MakeGenericType(action.GetDataType());
                    //var myList = Activator.CreateInstance(listType);
                    //var list = myList as System.Collections.IList;

                    //session.CreateCriteria(action.GetDataType())
                           //.Add(Restrictions.Eq("", ""))   //TODO: Can add filter/query items here
                           //.List(list);
                    var list = action.GetData(data);
                    action.ViewData = list;
                    result.Add(action);
                }
            }
            else if (eventItem is DoSomething)
            {
                var doResult = await (eventItem as DoSomething).ProcessAction(data);
                result.AddRange(doResult);
            }
            else if (eventItem is GetInput)
            {
                var inputResult = eventItem as GetInput;
                await inputResult.Initialize(data);
                result.Add(inputResult);
            }
            else if (eventItem is CancelInputDialog)
            {
                return Ok();
            }
            else
            {
                return BadRequest("ERROR: Unknown UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
            }
            return Json(result);
        }

        [HttpGet]
        [Route("getUserMenu")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> GetUserMenu()
        {
            var user = await this.GetLoggedInUserAsync();
            
            var results = new Dictionary<int, string>();
            using (var session = Store.OpenSession())
            {
                var roles = session.CreateCriteria<UserRoleAssociation>()
                                   .CreateAlias("User", "user")
                                   .Add(Restrictions.Eq("user.Id", user.Id))
                                   .List<UserRoleAssociation>()
                                   .ToList();

                var list = new List<Menu>();
                foreach (var role in roles)
                {
                    var tempQuery = session.QueryOver<Menu>().WhereRestrictionOn(x => x.UserRoleString).IsLike(role.UserRoleString);
                    list.AddRange(tempQuery.List<Menu>());
                }

                foreach (var menu in list)
                {
                    if (menu.Event != null && EventList.ContainsKey(menu.Event))
                    {
                        var eventItem = EventList[menu.Event];
                        results.Add((int)eventItem.GetId(), eventItem.Description);
                    }
                }
            }
            return Json(results);
        }
    }
}