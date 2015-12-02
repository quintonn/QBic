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


                    var adminRole = session.CreateCriteria<UserRole>()
                                           .Add(Restrictions.Eq("Name", "Admin"))
                                           .UniqueResult<UserRole>();
                    if (adminRole == null)
                    {
                        adminRole = new UserRole()
                        {
                            Name = "Admin",
                            Description = "Administrator"
                        };
                        session.Save(adminRole);
                    }

                    var adminRoleAssociation = session.CreateCriteria<UserRoleAssociation>()
                                                      .CreateAlias("User", "user")
                                                      .CreateAlias("UserRole", "role")
                                                      .Add(Restrictions.Eq("user.Id", adminUser.Id))
                                                      .Add(Restrictions.Eq("role.Id", adminRole.Id))
                                                      .UniqueResult<UserRoleAssociation>();
                    if (adminRoleAssociation == null)
                    {
                        adminRoleAssociation = new UserRoleAssociation()
                        {
                            User = adminUser,
                            UserRole = adminRole
                        };
                        session.Save(adminRoleAssociation);
                    }

                    var menuList1 = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.Eq("Event", EventNumber.ViewUsers))
                                           .List<Menu>();
                    if (menuList1.Count == 0)
                    {
                        var menu1 = new Menu()
                        {
                            Event = EventNumber.ViewUsers,
                            Name = "Users",
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
                            Name = "Menus",
                        };
                        session.Save(menu2);
                    }

                    var menuList3 = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.Eq("Event", EventNumber.ViewUserEvents))
                                           .List<Menu>();
                    if (menuList3.Count == 0)
                    {
                        var menu3 = new Menu()
                        {
                            Event = EventNumber.ViewUserEvents,
                            Name = "User Events",
                        };
                        session.Save(menu3);
                    }

                    var userRoleMenu = session.CreateCriteria<Menu>()
                                              .Add(Restrictions.Eq("Event", EventNumber.ViewUserRoles))
                                              .UniqueResult<Menu>();
                    if (userRoleMenu == null)
                    {
                        userRoleMenu = new Menu()
                        {
                            Event = EventNumber.ViewUserRoles,
                            Name = "User Roles"
                        };
                        session.Save(userRoleMenu);
                    }


                    var era1 = session.CreateCriteria<EventRoleAssociation>()
                                      .Add(Restrictions.Eq("Event", EventNumber.ViewEventRoleAssociations))
                                      .List<EventRoleAssociation>();
                    if (era1.Count == 0)
                    {
                        var evn = new EventRoleAssociation()
                        {
                            Event = EventNumber.ViewEventRoleAssociations,
                            UserRole = adminRole
                        };
                        session.Save(evn);
                    }

                    var era2 = session.CreateCriteria<EventRoleAssociation>()
                                      .Add(Restrictions.Eq("Event", EventNumber.AddEventRoleAssociation))
                                      .List<EventRoleAssociation>();
                    if (era2.Count == 0)
                    {
                        var evn = new EventRoleAssociation()
                        {
                            Event = EventNumber.AddEventRoleAssociation,
                            UserRole = adminRole
                        };
                        session.Save(evn);
                    }

                    var allEvents = Enum.GetValues(typeof(EventNumber)).Cast<int>().Where(e => e != (int)EventNumber.Nothing).ToList();
                    var eras = session.CreateCriteria<EventRoleAssociation>()
                                      .CreateAlias("UserRole", "role")
                                      .Add(Restrictions.Eq("role.Id", adminRole.Id))
                                      .List<EventRoleAssociation>()
                                      .ToList();
                    if (eras.Count != allEvents.Count)
                    {
                        eras.ForEach(e =>
                        {
                            session.Delete(e);
                        });
                        session.Flush();
                        foreach (var evt in allEvents)
                        {
                            var era = new EventRoleAssociation()
                            {
                                Event = (EventNumber)evt,
                                UserRole = adminRole
                            };
                            session.Save(era);
                        }
                    }

                    var testMenuList = session.CreateCriteria<Menu>()
                                              .Add(Restrictions.Eq("Name", "Test1"))
                                              .List<Menu>();
                    if (testMenuList.Count == 0)
                    {
                        var testMenu = new Menu()
                        {
                            Name = "Test1",
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
                                ParentMenu = testMenu,
                            };
                            session.Save(testMenu2);

                            var menu3 = new Menu()
                            {
                                Name = "Test3",
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
            var json = new
            {
                User = user.UserName,
                Role = "Admin",
                Id = user.Id
            };
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

                eventItem.Request = Request;
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

                    try
                    {
                        var list = action.GetData(parentData);
                        action.ViewData = list;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    var viewMenu = action.GetViewMenu();

                    var allowedEvents = GetAllowedEventsForUser(session, user.Id);
                    var allowedMenuItems = viewMenu.Where(m => allowedEvents.Contains(m.EventNumber)).ToList();

                    action.ViewMenu = allowedMenuItems; //TODO: this should work differently. because this can be changed in the view's code.

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

        private List<EventNumber> GetAllowedEventsForUser(ISession session, string userId)
        {
            var roles = session.CreateCriteria<UserRoleAssociation>()
                                   .CreateAlias("User", "user")
                                   .Add(Restrictions.Eq("user.Id", userId))
                                   .List<UserRoleAssociation>()
                                   .ToList();

            var userRoles = roles.Select(r => r.UserRole.Id).ToArray();
            var eventRoleAssociations = session.CreateCriteria<EventRoleAssociation>()
                                               .CreateAlias("UserRole", "role")
                                               .Add(Restrictions.In("role.Id", userRoles))
                                               .List<EventRoleAssociation>();

            var events = eventRoleAssociations.Select(e => e.Event).ToList();
            return events;
        }

        private void AddSubMenu(Menu menu, ISession session)
        {
            menu.ParentMenu = null;
            var subMenus = session.CreateCriteria<Menu>()
                                  .CreateAlias("ParentMenu", "parent")
                                  .Add(Restrictions.Eq("parent.Id", menu.Id))
                                  .List<Menu>()
                                  .ToList();
            foreach (var subMenu in subMenus)
            {
                AddSubMenu(subMenu, session);
            }
            menu.SubMenus = subMenus;
        }

        [HttpGet]
        [Route("getUserMenu")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> GetUserMenu()
        {
            var user = await this.GetLoggedInUserAsync();

            try
            {
                var results = new List<Menu>();
                using (var session = Store.OpenSession())
                {
                    var events = GetAllowedEventsForUser(session, user.Id).ToArray();

                    var mainMenus = session.CreateCriteria<Menu>()
                                               .Add(Restrictions.In("Event", events))
                                               .Add(Restrictions.IsNull("ParentMenu"))
                                               .List<Menu>()
                                               .ToList();
                    mainMenus.ForEach(m =>
                        {
                            results.Add(m);
                        });

                    var mainMenusWithSubMenus = session.CreateCriteria<Menu>()
                                               .Add(Restrictions.IsNull("Event"))
                                               .Add(Restrictions.IsNull("ParentMenu"))
                                               .List<Menu>()
                                               .ToList();
                    mainMenusWithSubMenus.ForEach(m =>
                    {
                        m.ParentMenu = null;
                        AddSubMenu(m, session);
                        results.Add(m);
                    });

                    //var subMenus = session.CreateCriteria<Menu>()
                    //                      .Add(Restrictions.In("Event", events))
                    //                      .Add(Restrictions.IsNotNull("ParentMenu"))
                    //                      .List<Menu>()
                    //                      .ToList();

                    //var x = -99;

                    //var submenusAdded = new List<string>();
                    //foreach (var subMenu in subMenus)
                    //{
                    //    var tmp = subMenu;
                    //    while (tmp.ParentMenu != null)
                    //    {
                    //        tmp = tmp.ParentMenu;
                    //    }
                    //    if (submenusAdded.Contains(tmp.Name))
                    //    {
                    //        continue;
                    //    }
                        
                    //    results.Add(x--, tmp.Name); //TODO: Here is should maybe build the sub-menu structure.  --> Maybe return the entire menu structure here.
                    //    submenusAdded.Add(tmp.Name);
                    //}
                }
                return Json(results);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}