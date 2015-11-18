using BasicAuthentication.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebsiteTemplate.Data;
using BasicAuthentication.ControllerHelpers;
using WebsiteTemplate.Models;
using NHibernate.Criterion;
using BasicAuthentication.Users;
using System.Diagnostics;
using System.Threading.Tasks;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.InputItems;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WebsiteTemplate.SiteSpecific.EventItems;

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
            PopulateEventList();
        }

        static MainController()
        {
            try
            {
                Log = new List<string>();
                CheckDefaultValues();
                //PopulateEventList(); /// If this is here, the menu descriptions get overriden. Need to fix this: TODO
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PopulateEventList()
        {
            EventList = new Dictionary<EventNumber, Event>();

            var types = typeof(Event).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Event)))
                {
                    var instance = (Event)Activator.CreateInstance(type);
                    instance.Store = Store;
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

                    var menuList1 = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.Eq("Event", EventNumber.ViewUsers))
                                           .List<Menu>();
                    if (menuList1.Count == 0)
                    {
                        var menu1 = new Menu()
                        {
                            Event = EventNumber.ViewUsers,
                            Name = "View Users",
                            AllowedUserRoles = new List<UserRole>() { UserRole.AnyOne, UserRole.ViewUsers }
                        };

                        session.Save(menu1);
                    }

                    var menuList2 = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.Eq("Event", EventNumber.ViewMenus))
                                           .List<Menu>();
                    if (menuList2.Count == 0)
                    {
                        var menu2 = new Menu()
                        {
                            Event = EventNumber.ViewMenus,
                            Name = "View Menus",
                            AllowedUserRoles = new List<UserRole>() { UserRole.AnyOne }
                        };
                        session.Save(menu2);
                    }

                    var testMenuList = session.CreateCriteria<Menu>()
                                              .Add(Restrictions.Eq("Name", "Test1"))
                                              .List<Menu>();
                    if (testMenuList.Count == 0)
                    {
                        var testMenu = new Menu()
                        {
                            Name = "Test1",
                            AllowedUserRoles = new List<UserRole>() { UserRole.AnyOne }
                        };
                        session.Save(testMenu);

                        var testMenuList2 = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.Eq("Name", "Test2"))
                                           .List<Menu>();
                        if (testMenuList2.Count == 0)
                        {
                            var testMenu2 = new Menu()
                            {
                                Name = "Test2",
                                AllowedUserRoles = new List<UserRole>() { UserRole.AnyOne },
                                ParentMenu = testMenu,
                            };
                            session.Save(testMenu2);

                            var menu3 = new Menu()
                            {
                                Name = "Test3",
                                AllowedUserRoles = new List<UserRole>() { UserRole.AnyOne },
                                ParentMenu = testMenu2,
                                Event = EventNumber.ViewMenus
                            };

                            session.Save(menu3);
                        }
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
                //var json = JsonConvert.DeserializeObject<JObject>(data);
                var json = JObject.Parse(data);
                
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
            var json = JObject.Parse(data);
            data = json.GetValue("Data").ToString();

            var id = (EventNumber)eventId;

            if (!EventList.ContainsKey(id))
            {
                return BadRequest("No action has been found for event number: " + id);
            }

            var result = new List<Event>();

            var eventItem = EventList[id];

            eventItem.Request = Request;

            if (eventItem is ShowView)
            {
                var action = eventItem as ShowView;

                using (var session = Store.OpenSession())
                {
                    var parentData = data;

                    var list = action.GetData(parentData);
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
                
                var initializeResult = await inputResult.Initialize(data);
                if (!initializeResult.Success)
                {
                    if (String.IsNullOrWhiteSpace(initializeResult.Error))
                    {
                        return BadRequest("There was an initialization error for GetInput " + eventItem.GetId() + " but there are not error details.");
                    }
                    return BadRequest(initializeResult.Error);
                }

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
                    var tempQuery = session.QueryOver<Menu>().WhereRestrictionOn(x => x.UserRoleString).IsLike("%" + role.UserRoleString + "%");
                    list.AddRange(tempQuery.List<Menu>());
                }

                var tQuery = session.QueryOver<Menu>().WhereRestrictionOn(x => x.UserRoleString).IsLike("%AnyOne%");
                list.AddRange(tQuery.List<Menu>());

                var xx = -1;
                foreach (var menu in list)
                {
                    if (EventList.ContainsKey(menu.Event))
                    {
                        var eventItem = EventList[menu.Event];
                        if (!results.ContainsKey((int)eventItem.GetId()))
                        {
                            results.Add((int)eventItem.GetId(), eventItem.Description);
                        }
                    }
                    else if (menu.Event == EventNumber.Nothing && menu.ParentMenu == null)
                    {
                        results.Add(xx--, menu.Name);
                    }
                }
            }
            return Json(results);
        }
    }
}