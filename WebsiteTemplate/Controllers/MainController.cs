using BasicAuthentication.Security;
using QCumber.Core.Utilities;
using log4net;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Unity;
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

        private static JsonSerializerSettings JSON_SETTINGS;

        private static bool Setup = false;
        private static object _Lock = new object();

        private static readonly ILog Logger = SystemLogger.GetLogger<MainController>();

        private string ConstructorError { get; set; }

        [HttpGet]
        [Route("custom/{*path}")]
        [AllowAnonymous]
        public IHttpActionResult DynamicRouteTest(string path) // This can be for an IEvent to handle exposing "Custom APIs"
        {
            return Ok("hello there\n" + path);
        }

        public MainController(IUnityContainer container)
        {
            try
            {
                lock (_Lock)
                {
                    Container = container;
                    ApplicationService = container.Resolve<ApplicationService>();
                    if (Setup == false)
                    {
                        Logger.Debug("MainController - Setup = false, performing setup");
                        var eventService = container.Resolve<EventService>(); // This is here to ensure EventService is initialize and it's constructor is called so that EventList is not empty

                        var dataService = container.Resolve<DataService>();

                        using (var session = dataService.OpenSession())
                        {
                            WebsiteUtils.DateFormat = "dd-MM-yyyy";
                            var appSettings = session.QueryOver<SystemSettings>().List<SystemSettings>().FirstOrDefault();
                            if (appSettings != null)
                            {
                                if (!String.IsNullOrWhiteSpace(WebsiteUtils.DateFormat))
                                {
                                    WebsiteUtils.DateFormat = appSettings.DateFormat;
                                }
                            }
                        }

                        ConstructorError = String.Empty;
                        Setup = true;
                    }

                    JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = WebsiteUtils.DateFormat };
                }
            }
            catch (Exception error)
            {
                SystemLogger.LogError<MainController>("Error in main controller constructor", error);
                ConstructorError = "";
                var err = error;
                while (err != null)
                {
                    ConstructorError = String.Format("{0}{1}\n{2}\n|---|\n", ConstructorError, error.Message, error.StackTrace);

                    err = error.InnerException;
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
            try
            {
                if (!String.IsNullOrWhiteSpace(ConstructorError))
                {
                    return BadRequest(ConstructorError);
                }
                await Container.Resolve<InitializationProcessor>().Process(0, Request); // Just to initialize core processor
                var json = ApplicationService.InitializeApplication(ConstructorError);
                if (JSON_SETTINGS != null)
                {
                    return Json(json, JSON_SETTINGS);
                }
                else
                {
                    return Json(json);
                }
            }
            catch (Exception error)
            {
                SystemLogger.LogError<MainController>("Error in initialize system", error);
                return BadRequest(error.Message + "\n" + error.StackTrace + "\n" + ConstructorError);
            }
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
            if (JSON_SETTINGS != null)
            {
                return Json(json, JSON_SETTINGS);
            }
            else
            {
                return Json(json);
            }
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
        [Authorize]
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

        [HttpPost]
        [Route("performBackup")]
        [RequireHttps]
        //[Authorize] //Not sure if we can have authorization. should  be possible
        //[DeflateCompression]
        public async Task<IHttpActionResult> PerformBackup()
        {
            return await Container.Resolve<BackupProcessor>().Process(-1, Request);
        }

        [HttpPost]
        [Route("setAcmeChallenge")]
        [RequireHttps]
        [AllowAnonymous]
        //[Authorize] //Not sure if we can have authorization. should  be possible
        public async Task<IHttpActionResult> SetAcmeChallenge()
        {
            //TODO:   1. Verify request using maybe key/value pair.
            //        2. Maybe just save to the correct path here with the correct file (maybe not, I can delete it using other way).
            try
            {
                string requestData;
                using (var stream = System.Web.HttpContext.Current.Request.InputStream)
                using (var mem = new System.IO.MemoryStream())
                {
                    stream.CopyTo(mem);
                    requestData = System.Text.Encoding.UTF8.GetString(mem.ToArray());
                }

                var acmeValue = requestData.Split("=".ToCharArray()).Last();

                //TODO:   The below code should also work?
                //        And, should also be sending full path
                Logger.Info("Setting acme challenge");
                Logger.Info("RequestData = " + requestData);
                try
                {
                    var jData = JsonHelper.Parse(requestData);
                    var acmeValueTmp = jData.GetValue("acme");
                    Logger.Info("acme value was found using json parser");

                    if (acmeValue == acmeValueTmp)
                    {
                        Logger.Info("Acme value is the same both ways");
                    }
                    else
                    {
                        Logger.Info("Acme challenge is not the same both ways");
                    }
                }
                catch (Exception error)
                {
                    Logger.Info("Acme value not json: " + error.Message);
                }

                var challengePath = "TODO";//

                AcmeController.ChallengeResponse = acmeValue;
                AcmeController.ChallengePath = challengePath;

                return Json("success: " + acmeValue);
            }
            catch (Exception error)
            {
                SystemLogger.LogError<MainController>("Error in setting acme challenge", error);
                return BadRequest("Unable to complete acme challenge: " + error.Message);
            }
        }
    }
}