using BasicAuthentication.Security;
using Microsoft.Practices.Unity;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Http;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        private IUnityContainer Container { get; set; }
        private ApplicationService ApplicationService { get; set; }

        private JsonSerializerSettings JSON_SETTINGS;

        [HttpGet]
        [Route("{*path}")]
        [AllowAnonymous]
        public IHttpActionResult DynamicRouteTest(string path) // This can be for an IEvent to handle exposing "Custom APIs"
        {
            return Ok("hello there\n" + path);
        }

        public MainController(IUnityContainer container)
        {
            Container = container;
            ApplicationService = container.Resolve<ApplicationService>();
            var eventService = container.Resolve<EventService>(); // This is here to ensure EventService is initialize and it's constructor is called so that EventList is not empty

            var dataService = container.Resolve<DataService>();
            using (var session = dataService.OpenSession())
            {
                var appSettings = session.QueryOver<SystemSettings>().List<SystemSettings>().FirstOrDefault();
                if (appSettings != null)
                {
                    JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = appSettings.DateFormat };
                }
                else
                {
                    JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = "yyyy-mm-dd" };
                }
            }
        }

        [HttpPost]
        [Route("systemPing")]
        [AllowAnonymous]
        [RequireHttps]
        [DeflateCompression]// - not sure how to deflate
        public async Task<IHttpActionResult> SystemPing()
        {
            return await Container.Resolve<PingProcessor>().Process(-1, Request);
        }

        [HttpGet]
        [Route("initializeSystem")]
        [AllowAnonymous]
        [RequireHttps]
        [DeflateCompression]
        public async Task<IHttpActionResult> InitializeSystem()
        {
            await Container.Resolve<InitializationProcessor>().Process(0, Request); // Just to initialize core processor

            var json = ApplicationService.InitializeApplication();
            return Json(json, JSON_SETTINGS);
        }

        [HttpGet]
        [Route("initialize")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> Initialize()
        {
            await Container.Resolve<InitializationProcessor>().Process(0, Request); // Just to initialize core processor

            var json = await ApplicationService.InitializeSession();
            return Json(json, JSON_SETTINGS);
        }

        [HttpPost]
        [Route("propertyChanged/{*eventId}")]
        [RequireHttps]
        [ConditionalAuthorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> OnPropertyChanged(int eventId)
        {
            return await Container.Resolve<PropertyChangeProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("processEvent/{*eventId}")]
        [RequireHttps]
        [ConditionalAuthorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ProcessEvent(int eventId)
        {
            return await Container.Resolve<InputEventProcessor>().Process(eventId, Request);
        }


        [HttpPost]
        [Route("GetFile/{*eventId}")]
        [RequireHttps]
        [ConditionalAuthorize]
        //[DeflateCompression] // Converts data to json which doesn't work for files
        public async Task<IHttpActionResult> GetFile(int eventId)
        {
            return await Container.Resolve<FileProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("updateViewData/{*eventId}")]
        [RequireHttps]
        [ConditionalAuthorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> UpdateViewData(int eventId)
        {
            return await Container.Resolve<UpdateViewProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("getViewMenu/{*eventId}")]
        [RequireHttps]
        [ConditionalAuthorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> GetViewMenu(int eventId)
        {
            return await Container.Resolve<ViewMenuProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        [RequireHttps]
        [ConditionalAuthorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ExecuteUIAction(int eventId)
        {
            return await Container.Resolve<ActionExecutionProcessor>().Process(eventId, Request);
        }

        [HttpGet]
        [Route("getUserMenu")]
        [RequireHttps]
        [Authorize] // TODO: We need a way to have menu's for when we don't require a logged in user.
        [DeflateCompression]
        public async Task<IHttpActionResult> GetUserMenu()
        {
            return await Container.Resolve<UserMenuProcessor>().Process(-1, Request);
        }
    }
}