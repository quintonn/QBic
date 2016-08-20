using BasicAuthentication.Security;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Data;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        private static IUnityContainer Container { get; set; }
        private static string ApplicationName { get; set; }

        private static DataStore Store { get; set; }
        private static ApplicationService ApplicationService { get; set; }

        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = "dd-MM-yyyy" };

        static MainController()
        {
            Store = new DataStore();

            Container = new UnityContainer();
            Container.LoadConfiguration();
            Container.RegisterInstance(Store);
            ApplicationService = Container.Resolve<ApplicationService>();

            var appSettings = Container.Resolve<IApplicationSettings>();
            appSettings.RegisterUnityContainers(Container);

            ApplicationName = appSettings.GetApplicationName();
        }

        [HttpGet]
        [Route("initializeSystem")]
        [AllowAnonymous]
        [RequireHttps]
        [DeflateCompression]
        public IHttpActionResult InitializeSystem()
        {
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
            var json = await ApplicationService.InitializeSession();
            return Json(json, JSON_SETTINGS);
        }

        [HttpPost]
        [Route("propertyChanged")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> OnPropertyChanged()
        {
            try
            {
                var result = await Container.Resolve<PropertyChangeProcessor>().Process();

                return Json(result, JSON_SETTINGS);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                var result = await Container.Resolve<InputEventProcessor>().Process(eventId);
                return Json(result, JSON_SETTINGS);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("GetFile/{*eventId}")]
        [RequireHttps]
        [Authorize]
        //[DeflateCompression] // Converts data to json which doesn't work for files
        public async Task<IHttpActionResult> GetFile(int eventId)
        {
            try
            {
                var fileInfo = await Container.Resolve<FileProcessor>().Process(eventId);

                return new FileActionResult(fileInfo);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
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
                var result = await Container.Resolve<UpdateViewProcessor>().Process(eventId);

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
            try
            {
                var allowedMenuItems = await Container.Resolve<ViewMenuProcessor>().Process(eventId);

                return Json(allowedMenuItems, JSON_SETTINGS);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> ExecuteUIAction(int eventId)
        {
            try
            {
                var result = await Container.Resolve<ActionExecutionProcessor>().Process(eventId);

                return Json(result, JSON_SETTINGS);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpGet]
        [Route("getUserMenu")]
        [RequireHttps]
        [Authorize]
        [DeflateCompression]
        public async Task<IHttpActionResult> GetUserMenu()
        {
            try
            {
                var results = await Container.Resolve<UserMenuProcessor>().Process();
                return Json(results, JSON_SETTINGS);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}