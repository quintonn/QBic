using BasicAuthentication.Security;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Http;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        private IUnityContainer Container { get; set; }
        private ApplicationService ApplicationService { get; set; }

        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = "dd-MM-yyyy" };

        public MainController(IUnityContainer container)
        {
            Container = container;
            ApplicationService = container.Resolve<ApplicationService>();
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
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> OnPropertyChanged(int eventId)
        {
            return await Container.Resolve<PropertyChangeProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("processEvent/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ProcessEvent(int eventId)
        {
            return await Container.Resolve<InputEventProcessor>().Process(eventId, Request);
        }


        [HttpPost]
        [Route("GetFile/{*eventId}")]
        [RequireHttps]
        [Authorize]
        //[DeflateCompression] // Converts data to json which doesn't work for files
        public async Task<IHttpActionResult> GetFile(int eventId)
        {
            return await Container.Resolve<FileProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("updateViewData/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> UpdateViewData(int eventId)
        {
            return await Container.Resolve<UpdateViewProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("getViewMenu/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> GetViewMenu(int eventId)
        {
            return await Container.Resolve<ViewMenuProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ExecuteUIAction(int eventId)
        {
            return await Container.Resolve<ActionExecutionProcessor>().Process(eventId, Request);
        }

        [HttpGet]
        [Route("getUserMenu")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> GetUserMenu()
        {
            return await Container.Resolve<UserMenuProcessor>().Process(-1, Request);
        }
    }
}