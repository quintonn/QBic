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

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        public static IDictionary<EventNumber, Event> EventList { get; set; }

        private DataStore Store { get; set; }

        public MainController()
        {
            Store = new DataStore();
        }

        static MainController()
        {
            try
            {
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
                    return Ok(new List<Event>()
                    {
                        new ShowMessage("No button with action number " + actionId + " exists for " + eventItem.Description),
                    });
                };
                var result = await eventItem.ProcessAction(formData, actionId);
                return Ok(result);
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
                    var listType = typeof(List<>).MakeGenericType(action.GetDataType());
                    var myList = Activator.CreateInstance(listType);
                    var list = myList as System.Collections.IList;

                    session.CreateCriteria(action.GetDataType())
                           //.Add(Restrictions.Eq("", ""))   //TODO: Can add filter/query items here
                           .List(list);
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
            return Ok(result);
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

                roles.ForEach(r =>
                    {
                        var menuIds = GetMenuItemsForAllRoles().Where(i => i.Key == r.UserRole || i.Key == UserRole.AnyOne).Select(i => i.Value).ToList();

                        menuIds.ForEach(i =>
                        {
                            if (!EventList.ContainsKey(i))
                            {
                                throw new NotImplementedException("No event exists for id " + i);
                            }
                            var eventItem = EventList[i];
                            
                            results.Add((int)eventItem.GetId(), eventItem.Description);
                        });
                    });
            }
            return Ok(results);
        }

        private Dictionary<UserRole, EventNumber> GetMenuItemsForAllRoles()
        {
            var results = new Dictionary<UserRole, EventNumber>();

            // So here i can create menus for users based on their roles.
            // So if a user is in 2 roles, he will get all of those menu items.
            //results.Add("User Role", UIActionId);

            //EG
            results.Add(UserRole.ViewUsers, EventNumber.ViewUsers);

            results.Add(UserRole.AnyOne, EventNumber.ViewUserRoleAssociations);

            return results;
        }

    }
}