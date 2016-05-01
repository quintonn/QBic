﻿using BasicAuthentication.Security;
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

using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.InputItems;
using Newtonsoft.Json.Linq;

using NHibernate;
using System.IO;
using System.Reflection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using WebsiteTemplate.SiteSpecific.Utilities;
using System.Transactions;
using WebsiteTemplate.Menus.BasicCrudItems;
using WebsiteTemplate.Backend.TestItem;
using Newtonsoft.Json;
using WebsiteTemplate.Backend.Users;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        public static IDictionary<int, Event> EventList { get; set; }

        public static List<string> Log { get; set; }

        private DataStore Store { get; set; }

        private static UnityContainer Container { get; set; }
        private static string ApplicationName { get; set; }

        public MainController()
        {
            Store = new DataStore();
            PopulateEventList();

            CheckDefaultValues();
        }

        static MainController()
        {
            try
            {
                EventList = new Dictionary<int, Event>();

                Container = new UnityContainer();
                Container.LoadConfiguration();

                var appSettings = Container.Resolve<IApplicationSettings>();
                ApplicationName = appSettings.GetApplicationName();
                appSettings.RegisterUnityContainers(Container);

                Log = new List<string>();

                //CheckDefaultValues();
                //PopulateEventList(); /// If this is here, the menu descriptions get overriden. Need to fix this: TODO
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PopulateEventList()
        {
            if (EventList.Count > 0)
            {
                //return;
                EventList = new Dictionary<int, Event>();
            }

            
            var curDir = System.Web.HttpRuntime.AppDomainAppPath;
            var dlls = Directory.GetFiles(curDir, "*.dll", SearchOption.AllDirectories);
            var types = new List<Type>();

            var appDomain = AppDomain.CreateDomain("tmpDomainForWebTemplate");
            foreach (var dll in dlls)
            {
                if (dll.Contains("\\roslyn\\"))
                {
                    continue;
                }
                try
                {
                    var assembly = appDomain.Load(File.ReadAllBytes(dll));
                
                
                    var eventTypes = assembly.GetTypes()
                                            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Event)))
                                            .ToList();
                    if (eventTypes.Count > 0)
                    {
                        types.AddRange(eventTypes);
                    }
                    
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            AppDomain.Unload(appDomain);

            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Event)))
                {
                    if (type == typeof(BasicCrudModify<>))
                    {
                        continue;
                    }
                    if (type == typeof(BasicCrudView<>))
                    {
                        continue;
                    }
                    if (type == typeof(BasicCrudDelete<>))
                    {
                        continue;
                    }


                    if (type.GetInterface("IBasicCrudMenuItem") != null)
                    {
                        var subType = (IBasicCrudMenuItem)Activator.CreateInstance(type);

                        var d1 = typeof(BasicCrudView<>);
                        Type[] typeArgs1 = { subType.InnerType };
                        var viewType = d1.MakeGenericType(typeArgs1);

                        var viewInstance = (Event)Activator.CreateInstance(viewType, subType.GetBaseMenuId(), subType.GetBaseItemName(), subType.GetColumnsToShowInView());
                        viewInstance.Store = Store;
                        if (!EventList.ContainsKey(viewInstance.GetId()))
                        {
                            EventList.Add(viewInstance.GetId(), viewInstance);
                        }

                        var d2 = typeof(BasicCrudModify<>);
                        Type[] typeArgs2 = { subType.InnerType };
                        var modifyType = d2.MakeGenericType(typeArgs2);
                        var modifyInstance = (Event)Activator.CreateInstance(modifyType, subType.GetBaseMenuId() + 1, subType.GetBaseItemName(), subType.GetInputProperties());
                        modifyInstance.Store = Store;
                        if (!EventList.ContainsKey(modifyInstance.GetId()))
                        {
                            EventList.Add(modifyInstance.GetId(), modifyInstance);
                        }

                        var d3 = typeof(BasicCrudDelete<>);
                        Type[] typeArgs3 = { subType.InnerType };
                        var deleteType = d3.MakeGenericType(typeArgs2);
                        var deleteInstance = (Event)Activator.CreateInstance(deleteType, subType.GetBaseMenuId() + 2, subType.GetBaseItemName());
                        deleteInstance.Store = Store;
                        if (!EventList.ContainsKey(deleteInstance.GetId()))
                        {
                            EventList.Add(deleteInstance.GetId(), deleteInstance);
                        }
                    }
                    else if (type != typeof(BasicCrudMenuItem<>))
                    {
                        var instance = (Event)Activator.CreateInstance(type);
                        instance.Store = Store;
                        if (!EventList.ContainsKey(instance.GetId()))
                        {
                            EventList.Add(instance.GetId(), instance);
                        }
                    }
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

                    var allEvents = EventList.Select(e => Convert.ToInt32(e.Value.GetEventId()))
                                          .ToList();

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
                                Event = evt,
                                UserRole = adminRole
                            };
                            session.Save(era);
                        }
                    }

                    Container.Resolve<IApplicationSettings>().SetupDefaults(session);

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
        [Route("initializeSystem")]
        [RequireHttps]
        public IHttpActionResult InitializeSystem()
        {
            var json = new
            {
                ApplicationName = ApplicationName
            };
            return Json(json);
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
                Id = user.Id,
            };
            return Json(json);
        }

        [HttpPost]
        [Route("propertyChanged")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> OnPropertyChanged()
        {
            var data = await Request.Content.ReadAsStringAsync();
            var json = JObject.Parse(data);
            data = json.GetValue("Data").ToString();

            json = JObject.Parse(data);

            var eventId = Convert.ToInt32(json.GetValue("EventId"));
            var propertyName = json.GetValue("PropertyName").ToString();
            var propertyValue = json.GetValue("PropertyValue") as object;
            var eventItem = EventList[eventId] as GetInput;

            var result = await eventItem.OnPropertyChanged(propertyName, propertyValue);

            return Json(result);
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

                var id = eventId;
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
                IList<Event> result;

                var jsonData = JObject.Parse(formData);
                //var processedFormData = formData;
                var processedFormData = new Dictionary<string, object>();
                foreach (var inputField in eventItem.InputFields)
                {
                    var value = inputField.GetValue(jsonData.GetValue(inputField.InputName));
                    processedFormData.Add(inputField.InputName, value);
                }
                using (var session = Store.OpenSession())
                {
                    eventItem.InputData = processedFormData;
                    result = await eventItem.ProcessAction(actionId);
                    session.Flush();
                }
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
            try
            {
                var user = await this.GetLoggedInUserAsync();
                var data = await Request.Content.ReadAsStringAsync();
                var json = JObject.Parse(data);
                data = json.GetValue("Data").ToString();

                var id = eventId;

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
                    var processedFormData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                    if (processedFormData.ContainsKey("rowData"))
                    {
                        var rowData = processedFormData["rowData"].ToString();
                        string rowId = "";
                        if (processedFormData.ContainsKey("rowId"))
                        {
                            rowId = processedFormData["rowId"].ToString();
                        }
                        processedFormData = JsonConvert.DeserializeObject<Dictionary<string, object>>(rowData);
                        if (!String.IsNullOrWhiteSpace(rowId))
                        {
                            if (!processedFormData.ContainsKey("rowId"))
                            {
                                processedFormData.Add("rowId", rowId); /// This is a hack, fix this!
                            }
                        }
                    }

                    (eventItem as DoSomething).InputData = processedFormData;
                    var doResult = await (eventItem as DoSomething).ProcessAction();
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
                            return BadRequest("There was an initialization error for ExecuteUIAction " + eventItem.GetId() + " but there are not error details.");
                        }
                        return BadRequest(initializeResult.Error);
                    }

                    result.Add(inputResult);
                }
                else if (eventItem is CancelInputDialog)
                {
                    return Ok();
                }
                else if (eventItem is OpenFile)
                {
                    (eventItem as OpenFile).SetData(data);
                    result.Add(eventItem);
                }
                else
                {
                    return BadRequest("ERROR: Unknown UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
                }

                return Json(result);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        private List<int> GetAllowedEventsForUser(ISession session, string userId)
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