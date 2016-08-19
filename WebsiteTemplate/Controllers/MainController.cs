﻿using BasicAuthentication.ControllerHelpers;
using BasicAuthentication.Security;
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
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        public static IDictionary<int, IEvent> EventList { get; set; }

        public static List<string> Log { get; set; }

        private static DataStore Store { get; set; }

        private static IUnityContainer Container { get; set; }
        private static string ApplicationName { get; set; }

        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = "dd-MM-yyyy" };

        public MainController()
        {
            //CheckDefaultValues();
            Console.WriteLine("Temp");
        }

        static MainController()
        {
            Store = new DataStore();
            EventList = new Dictionary<int, IEvent>();

            Container = new UnityContainer();
            Container.LoadConfiguration();
            Container.RegisterInstance(Store);

            var appSettings = Container.Resolve<IApplicationSettings>();
            ApplicationName = appSettings.GetApplicationName();
            appSettings.RegisterUnityContainers(Container);

            Log = new List<string>();
            CheckDefaultValues();
            PopulateEventList();
        }

        private static void PopulateEventList()
        {
            if (EventList.Count > 0)
            {
                return;
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
                        var subType = (IBasicCrudMenuItem)Container.Resolve(type);

                        var d1 = typeof(BasicCrudView<>);
                        Type[] typeArgs1 = { subType.InnerType };
                        var viewType = d1.MakeGenericType(typeArgs1);

                        var viewInstance = (IBasicCrudView)Container.Resolve(viewType);
                        viewInstance.Id = subType.GetBaseMenuId();
                        viewInstance.ItemName = subType.GetBaseItemName();
                        viewInstance.ColumnsToShowInView = subType.GetColumnsToShowInView();

                        if (!EventList.ContainsKey(viewInstance.GetId()))
                        {
                            EventList.Add(viewInstance.GetId(), viewInstance as Event);
                        }

                        var d2 = typeof(BasicCrudModify<>);
                        Type[] typeArgs2 = { subType.InnerType };
                        var modifyType = d2.MakeGenericType(typeArgs2);
                        var modifyInstance = (IBasicCrudModify)Container.Resolve(modifyType);
                        modifyInstance.Id = subType.GetBaseMenuId() + 1;
                        modifyInstance.ItemName = subType.GetBaseItemName();
                        modifyInstance.InputProperties = subType.GetInputProperties();
                        if (!EventList.ContainsKey(modifyInstance.GetId()))
                        {
                            EventList.Add(modifyInstance.GetId(), modifyInstance as Event);
                        }

                        var d3 = typeof(BasicCrudDelete<>);
                        Type[] typeArgs3 = { subType.InnerType };
                        var deleteType = d3.MakeGenericType(typeArgs2);
                        var deleteInstance = (IBasicCrudDelete)Container.Resolve(deleteType);
                        deleteInstance.Id = subType.GetBaseMenuId() + 2;
                        deleteInstance.ItemName = subType.GetBaseItemName();
                        if (!EventList.ContainsKey(deleteInstance.GetId()))
                        {
                            EventList.Add(deleteInstance.GetId(), deleteInstance as Event);
                        }
                    }
                    else if (type != typeof(BasicCrudMenuItem<>))
                    {
                        var instance = (IEvent)Container.Resolve(type);
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
            try
            {
                //TODO: This should be in a class somewhere to be overriden for actual implementations
                using (var session = Store.OpenSession())
                {
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
        [DeflateCompression]
        public IHttpActionResult InitializeSystem()
        {
            var version = typeof(ShowView).Assembly.GetName().Version.ToString();

            var json = new
            {
                ApplicationName = ApplicationName,
                Version = version
            };
            return Json(json, JSON_SETTINGS);
        }

        [HttpGet]
        [Route("initialize")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public IHttpActionResult Initialize()
        {
            var user = this.GetLoggedInUser() as User;
            var json = new
            {
                User = user.UserName,
                Role = "Admin",
                Id = user.Id,
            };
            return Json(json, JSON_SETTINGS);
        }

        [HttpPost]
        [Route("propertyChanged")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> OnPropertyChanged()
        {
            var data = await Request.Content.ReadAsStringAsync();
            var json = JsonHelper.Parse(data);
            data = json.GetValue("Data");

            json = JsonHelper.Parse(data);

            var eventId = json.GetValue<int>("EventId");
            var propertyName = json.GetValue("PropertyName");
            var propertyValue = json.GetValue<object>("PropertyValue");
            var eventItem = EventList[eventId] as GetInput;

            var result = await eventItem.OnPropertyChanged(propertyName, propertyValue);

            return Json(result, JSON_SETTINGS);
        }

        [HttpPost]
        [Route("processEvent/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ProcessEvent(int eventId)
        {
            try
            {
                var data = await Request.Content.ReadAsStringAsync();
                var json = JsonHelper.Parse(data);

                //var parameters = json.ToObject<Dictionary<string, object>>();
                var formData = json.GetValue("Data");
                var actionId = json.GetValue<int>("ActionId");

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

                var callParameters = String.Empty;

                var jsonData = JsonHelper.Parse(formData);
                callParameters = jsonData.GetValue("parameters");

                IList<Event> result;

                var processedFormData = new Dictionary<string, object>();
                
                await eventItem.Initialize(formData);
                foreach (var inputField in eventItem.InputFields)
                {
                    var value = inputField.GetValue(jsonData.GetValue<JToken>(inputField.InputName));
                    processedFormData.Add(inputField.InputName, value);
                }
                using (var session = Store.OpenSession())
                {
                    eventItem.InputData = processedFormData;
                    eventItem.Store = Store;
                    result = await eventItem.ProcessAction(actionId);
                   
                    HandleProcessActionResult(result, eventItem);
                    session.Flush();
                }
                foreach (var item in result)
                {
                    item.Parameters = callParameters;
                }
                return Json(result, JSON_SETTINGS);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void HandleProcessActionResult(IList<Event> result, IEvent eventItem)
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
                        throw new Exception("Put rowid code back");
                        //(item as UpdateInputView).RowId = Convert.ToInt32(inputData["rowId"]);
                    }
                    if (String.IsNullOrWhiteSpace(jsonDataToUpdate))
                    {
                        if (inputData.ContainsKey("ViewData"))
                        {
                            inputData.Remove("ViewData");
                        }
                        jsonDataToUpdate = JsonHelper.FromObject(inputData).ToString();
                    }
                    (item as UpdateInputView).JsonDataToUpdate = jsonDataToUpdate;
                }
            }
        }


        [HttpPost]
        [Route("GetFile/{*eventId}")]
        [RequireHttps]
        [Authorize]
        //[DeflateCompression] // Converts data to json which doesn't work for files
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

        [HttpPost]
        [Route("updateViewData/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> UpdateViewData(int eventId)
        {
            try
            {
                var user = await this.GetLoggedInUserAsync();
                var originalData = await Request.Content.ReadAsStringAsync();

                var json = JsonHelper.Parse(originalData);
                originalData = json.GetValue("Data");

                var data = originalData;
                if (!String.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        var tmp = JsonHelper.Parse(data);
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

                Event result;

                var eventItem = EventList[id];

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

                        var dataJson = new JsonHelper();
                        if (!String.IsNullOrWhiteSpace(data))
                        {
                            try
                            {
                                dataJson = JsonHelper.Parse(data);

                                filter = dataJson.GetValue("filter");
                                parameters = dataJson.GetValue("parameters");

                                var viewSettings = dataJson.GetValue<JsonHelper>("viewSettings");
                                if (viewSettings != null)
                                {
                                    currentPage = viewSettings.GetValue<int>("currentPage");
                                    linesPerPage = viewSettings.GetValue<int>("linesPerPage");
                                    if (linesPerPage == -2)
                                    {
                                        currentPage = 1; //just in case it's not
                                        linesPerPage = int.MaxValue;
                                    }
                                    totalLines = viewSettings.GetValue<int>("totalLines");
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

                        totalLines = Math.Max(totalLines, list.Cast<object>().Count());

                        action.CurrentPage = currentPage;
                        action.LinesPerPage = linesPerPage == int.MaxValue ? -2 : linesPerPage;
                        action.TotalLines = totalLines;
                        action.Filter = filter;
                        action.Parameters = parameters;

                        result = action;
                    }
                }
                else
                {
                    return BadRequest("ERROR: Invalid UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
                }
                
                return Json(result, JSON_SETTINGS);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpPost]
        [Route("getViewMenu/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> GetViewMenu(int eventId)
        {
            var postData = await Request.Content.ReadAsStringAsync();
            var data = String.Empty;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                var json = JsonHelper.Parse(postData);
                data = json.GetValue("data");
            }

            if (!EventList.ContainsKey(eventId))
            {
                return BadRequest("No action has been found for event number: " + eventId);
            }

            var eventItem = EventList[eventId] as ShowView;

            var dataForMenu = new Dictionary<string, string>();
            if (!String.IsNullOrWhiteSpace(data))
            {
                dataForMenu = JsonHelper.DeserializeObject<Dictionary<string, string>>(data);
            }
            try
            {
                var viewMenu = eventItem.GetViewMenu(dataForMenu);

                var user = await this.GetLoggedInUserAsync();
                List<MenuItem> allowedMenuItems;
                using (var session = Store.OpenSession())
                {

                    var allowedEvents = GetAllowedEventsForUser(session, user.Id);
                    allowedMenuItems = viewMenu.Where(m => allowedEvents.Contains(m.EventNumber)).ToList();
                }

                return Json(allowedMenuItems, JSON_SETTINGS);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /*      When clicking a create button on an input view, i don't have access to any information on the screen
                I might need this (eg, in the claims for creating note attachment. Because i want to check if there 
                is already a note with the name note1.txt, and if there is, make it note2.txt, etc.
        */

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ExecuteUIAction(int eventId)
        {
            try
            {
                // TODO: Need a way to confirm if user is allowed to execute this event
                //       Make re-usable so it can be put into views and input screens for buttons and links

                var user = await this.GetLoggedInUserAsync();
                var originalData = await Request.Content.ReadAsStringAsync();
                
                var json = JsonHelper.Parse(originalData);
                originalData = json.GetValue("Data");

                var data = originalData;
                if (!String.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        var tmp = JsonHelper.Parse(data);
                        if (tmp != null)
                        {
                            var subData = tmp.GetValue("data");
                            
                            if (!String.IsNullOrWhiteSpace(subData))
                            {
                                data = subData;
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

                var tmpJson = JsonHelper.Parse(originalData);
                var parameters = tmpJson.GetValue("parameters");

                var id = eventId;

                if (!EventList.ContainsKey(id))
                {
                    return BadRequest("No action has been found for event number: " + id);
                }

                var result = new List<IEvent>();

                var eventItem = EventList[id];

                if (eventItem is ShowView)
                {
                    var action = eventItem as ShowView;

                    using (var session = Store.OpenSession())
                    {
                        data = originalData;
                        var parentData = data;

                        var dataJson = new JsonHelper();
                        if (!String.IsNullOrWhiteSpace(data) && !(eventItem is ViewForInput))
                        {
                            dataJson = JsonHelper.Parse(data);
                            parameters = dataJson.GetValue("parameters");
                        }
                        else
                        {
                            parentData = data;  // In case user modified parentData -> this smells??
                        }

                        var totalLines = action.GetDataCount(parentData, String.Empty);

                        var list = action.GetData(parentData, 1, 10, String.Empty);
                        action.ViewData = list;

                        totalLines = Math.Max(totalLines, list.Cast<object>().Count());

                        action.CurrentPage = 1;
                        action.LinesPerPage = 10;
                        action.TotalLines = totalLines;
                        action.Filter = String.Empty;
                      
                        //clicking back in view many times breaks filter args- test with menus

                        result.Add(action);
                    }
                }
                else if (eventItem is DoSomething)
                {
                    var processedFormData = JsonHelper.DeserializeObject<Dictionary<string, object>>(data);
                    if (processedFormData != null && processedFormData.ContainsKey("rowData"))
                    {
                        throw new Exception("Put rowData code back");
                        //var rowData = processedFormData["rowData"].ToString();
                        //processedFormData = JsonHelper.DeserializeObject<Dictionary<string, object>>(rowData);
                    }
                    else if (processedFormData == null)
                    {
                        processedFormData = new Dictionary<string, object>(); // Cannot/should not be null
                    }

                    if (!processedFormData.ContainsKey("ViewData") && !String.IsNullOrWhiteSpace(data))
                    {
                        var viewData = String.Empty;
                        var tmpData = JsonHelper.Parse(data);
                        viewData = tmpData.GetValue("ViewData");
                        processedFormData.Add("ViewData", viewData);
                    }

                    (eventItem as DoSomething).InputData = processedFormData;
                    (eventItem as DoSomething).Store = Store;
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
                    (eventItem as UserConfirmation).Data = originalData;
                    result.Add(eventItem);
                }
                else
                {
                    return BadRequest("ERROR: Unknown UIActionType: " + eventItem.GetType().ToString().Split(".".ToCharArray()).Last() + " with id " + id);
                }

                foreach (var item in result)
                {
                    item.Parameters = parameters;
                }

                return Json(result, JSON_SETTINGS);
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
        [DeflateCompression]
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
                return Json(results, JSON_SETTINGS);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}