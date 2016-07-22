using BasicAuthentication.ControllerHelpers;
using BasicAuthentication.Security;
using BasicAuthentication.Users;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

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
        [AllowAnonymous]
        [RequireHttps]
        public IHttpActionResult InitializeSystem()
        {
            var version = typeof(ShowView).Assembly.GetName().Version.ToString();

            var json = new
            {
                ApplicationName = ApplicationName,
                Version = version
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
                var processedFormData = new Dictionary<string, object>();
                
                await eventItem.Initialize(formData);
                foreach (var inputField in eventItem.InputFields)
                {
                    var value = inputField.GetValue(jsonData.GetValue(inputField.InputName));
                    processedFormData.Add(inputField.InputName, value);
                }
                using (var session = Store.OpenSession())
                {
                    eventItem.InputData = processedFormData;
                    result = await eventItem.ProcessAction(actionId);
                   
                    HandleProcessActionResult(result, eventItem);
                    session.Flush();
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void HandleProcessActionResult(IList<Event> result, Event eventItem)
        {
            var jsonDataToUpdate = String.Empty;
            foreach (var item in result)
            {
                if (item is UpdateInputView)
                {
                    Dictionary<string, object> inputData;
                    if (eventItem is GetInput)
                    {
                        inputData = (eventItem as GetInput).InputData;
                    }
                    else if (eventItem is InputProcessingEvent)
                    {
                        inputData = (eventItem as InputProcessingEvent).InputData;
                    }
                    else
                    {
                        throw new Exception("Unknown eventItem for UpdateInputView result: " + eventItem.GetType().ToString());
                    }
                    if (inputData.ContainsKey("rowId"))
                    {
                        (item as UpdateInputView).RowId = Convert.ToInt32(inputData["rowId"]);
                    }
                    if (String.IsNullOrWhiteSpace(jsonDataToUpdate))
                    {
                        jsonDataToUpdate = JObject.FromObject(inputData).ToString();
                    }
                    (item as UpdateInputView).JsonDataToUpdate = jsonDataToUpdate;
                }
            }
        }


        [HttpPost]
        [Route("GetFile/{*eventId}")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> GetFile(int eventId)
        {
            var data = await Request.Content.ReadAsStringAsync();

            if (!EventList.ContainsKey(eventId))
            {
                return BadRequest("No action has been found for event number: " + eventId);
            }

            var eventItem = EventList[eventId] as OpenFile;

            var fileInfo = eventItem.GetFileInfo(data);

            return new FileActionResult(fileInfo);
        }

        /*      When clicking a create button on an input view, i don't have access to any information on the screen
                I might need this (eg, in the claims for creating note attachment. Because i want to check if there 
                is already a note with the name note1.txt, and if there is, make it note2.txt, etc.
        */

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        [RequireHttps]
        [Authorize]
        public async Task<IHttpActionResult> ExecuteUIAction(int eventId)
        {
            try
            {
                var user = await this.GetLoggedInUserAsync();
                var originalData = await Request.Content.ReadAsStringAsync();
                
                var json = JObject.Parse(originalData);
                originalData = json.GetValue("Data").ToString();

                var data = originalData;
                if (!String.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        var tmp = JObject.Parse(data) as JObject;
                        if (tmp != null)
                        {
                            var subData = tmp.GetValue("data");
                            if (subData != null)
                            {
                                data = subData.ToString();
                            }
                        }
                    }
                    catch (Newtonsoft.Json.JsonReaderException ex)
                    {
                        //do nothing, data was not json data.
                        Console.WriteLine(ex.Message);
                        //data = "";
                    }
                }

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
                        data = originalData;
                        var parentData = data;

                        var currentPage = 1;
                        var linesPerPage = 10;
                        var totalLines = -1;
                        var filter = String.Empty;
                        var parameters = String.Empty;

                        var dataJson = new JObject();
                        if (!String.IsNullOrWhiteSpace(data) && !(eventItem is ViewForInput))
                        {
                            try
                            {
                                dataJson = JObject.Parse(data);

                                filter = dataJson.GetValue("filter")?.ToString();
                                parameters = dataJson.GetValue("parameters")?.ToString();

                                var viewSettings = dataJson.GetValue("viewSettings") as JObject;
                                if (viewSettings != null)
                                {
                                    currentPage = Convert.ToInt32(viewSettings.GetValue("currentPage"));
                                    linesPerPage = Convert.ToInt32(viewSettings.GetValue("linesPerPage"));
                                    if (linesPerPage > 100)
                                    {
                                        currentPage = 1; //just in case it's not
                                        linesPerPage = int.MaxValue;
                                    }
                                    totalLines = Convert.ToInt32(viewSettings.GetValue("totalLines"));
                                }
                                if (filter == "nonononono")
                                {
                                    parentData = dataJson.GetValue("data")?.ToString();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                        {
                            parentData = data;  // In case user modified parentData -> this smells??
                        }

                        if (totalLines == -1 || !String.IsNullOrWhiteSpace(filter))
                        {
                            totalLines = action.GetDataCount(parentData, filter);
                        }

                        var list = action.GetData(parentData, currentPage, linesPerPage, filter);
                        action.ViewData = list;

                        var viewMenu = action.GetViewMenu();

                        var allowedEvents = GetAllowedEventsForUser(session, user.Id);
                        var allowedMenuItems = viewMenu.Where(m => allowedEvents.Contains(m.EventNumber)).ToList();

                        action.ViewMenu = allowedMenuItems; //TODO: this should work differently. because this can be changed in the view's code.

                        action.CurrentPage = currentPage;
                        action.LinesPerPage = linesPerPage;
                        action.TotalLines = totalLines;
                        action.Filter = filter;
                        action.Parameters = parameters;

                        //clicking back in view many times breaks filter args- test with menus

                        result.Add(action);
                    }
                }
                else if (eventItem is DoSomething)
                {
                    var processedFormData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                    if (processedFormData != null && processedFormData.ContainsKey("rowData"))
                    {
                        var rowData = processedFormData["rowData"].ToString();

                        processedFormData = JsonConvert.DeserializeObject<Dictionary<string, object>>(rowData);
                    }
                    else
                    {
                        processedFormData = new Dictionary<string, object>();
                    }

                    (eventItem as DoSomething).InputData = processedFormData;
                    var doResult = await (eventItem as DoSomething).ProcessAction();
                    HandleProcessActionResult(doResult, eventItem);
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
                else if (eventItem is UserConfirmation)
                {
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