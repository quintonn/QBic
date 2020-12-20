﻿using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QBic.Core.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    [Route("api/v1")]
    public class MainController : ControllerBase
    {
        private IServiceProvider Container { get; set; }
        private ApplicationService ApplicationService { get; set; }

        private static JsonSerializerOptions JSON_SETTINGS;

        private static bool Setup = false;
        private static object _Lock = new object();

        private static readonly ILog Logger = SystemLogger.GetLogger<MainController>();

        private string ConstructorError { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        [HttpGet]
        [Route("custom/{*path}")]
        [AllowAnonymous]
        public IActionResult DynamicRouteTest(string path) // This can be for an IEvent to handle exposing "Custom APIs"
        {
            return Ok("hello there\n" + path);
        }
        public static int XXX = 0;
        public MainController(IServiceProvider container, IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
            try
            {
                lock (_Lock)
                {
                    Container = container;
                    ApplicationService = container.GetService<ApplicationService>();
                    if (Setup == false)
                    {
                        Logger.Debug("MainController - Setup = false, performing setup");
                        var eventService = container.GetService<EventService>(); // This is here to ensure EventService is initialize and it's constructor is called so that EventList is not empty

                        var dataService = container.GetService<DataService>();

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

                    JSON_SETTINGS = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };// { DateFormatString = WebsiteUtils.DateFormat };
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
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        public async Task<IActionResult> SystemPing()
        {
            return await Container.GetService<PingProcessor>().Process(-1, this.Request);
        }

        [HttpGet]
        [Route("initializeSystem")]
        //[AllowAnonymous]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        public async Task<IActionResult> InitializeSystem()
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(ConstructorError))
                {
                    return new BadRequestObjectResult(ConstructorError);
                }
                await Container.GetService<InitializationProcessor>().Process(0, Request); // Just to initialize core processor
                var json = ApplicationService.InitializeApplication(ConstructorError);
                //json = JsonSerializer.Serialize(json);
                JsonResult result;
                if (JSON_SETTINGS != null)
                {
                    result = new JsonResult(json);//, JSON_SETTINGS));
                    
                }
                else
                {
                    result = new JsonResult(json);//JsonSerializer.Serialize(json));
                    
                }
                result.ContentType = "application/json";
                return result;
            }
            catch (Exception error)
            {
                SystemLogger.LogError<MainController>("Error in initialize system", error);
                return BadRequest(error.Message + "\n" + error.StackTrace + "\n" + ConstructorError);
            }
        }

        [HttpGet]
        [Route("initialize")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [Authorize]
        public async Task<IActionResult> Initialize()
        {
            await Container.GetService<InitializationProcessor>().Process(0, Request); // Just to initialize core processor

            var json = await ApplicationService.InitializeSession();
            if (JSON_SETTINGS != null)
            {
                return new JsonResult(json)
                {
                    ContentType = "application/json"
                };//, JSON_SETTINGS);
            }
            else
            {
                return new JsonResult(json)
                {
                    ContentType = "application/json"
                };
            }
        }

        [HttpPost]
        [Route("propertyChanged/{*eventId}")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [ConditionalAuthorize]
        public async Task<IActionResult> OnPropertyChanged(int eventId)
        {
            return await Container.GetService<PropertyChangeProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("processEvent/{*eventId}")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [ConditionalAuthorize]
        public async Task<IActionResult> ProcessEvent(int eventId)
        {
            return await Container.GetService<InputEventProcessor>().Process(eventId, Request);
        }


        [HttpPost]
        [Route("GetFile/{*eventId}")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [Authorize]
        ////[DeflateCompression] // Converts data to json which doesn't work for files
        public async Task<IActionResult> GetFile(int eventId)
        {
            var result = await Container.GetService<FileProcessor>().Process(eventId, Request);
            if (result is FileContentResult)
            {
                var f = result as FileContentResult;
                Response.Headers.Add("filename", f.FileDownloadName);
            }
            return result;
        }

        [HttpPost]
        [Route("updateViewData/{*eventId}")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [ConditionalAuthorize]
        public async Task<IActionResult> UpdateViewData(int eventId)
        {
            return await Container.GetService<UpdateViewProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("getViewMenu/{*eventId}")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [ConditionalAuthorize]
        public async Task<IActionResult> GetViewMenu(int eventId)
        {
            return await Container.GetService<ViewMenuProcessor>().Process(eventId, Request);
        }

        [HttpPost]
        [Route("executeUIAction/{*eventId}")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [ConditionalAuthorize]
        public async Task<IActionResult> ExecuteUIAction(int eventId)
        {
            return await Container.GetService<ActionExecutionProcessor>().Process(eventId, Request);
        }

        [HttpGet]
        [Route("getUserMenu")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [Authorize] // TODO: We need a way to have menu's for when we don't require a logged in user.
        public async Task<IActionResult> GetUserMenu()
        {
            var result = await Container.GetService<UserMenuProcessor>().Process(-1, Request);
            
            return result;
        }

        [HttpPost]
        [Route("performBackup")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        //[Authorize] //Not sure if we can have authorization. should  be possible
        ////[DeflateCompression]
        public async Task<IActionResult> PerformBackup()
        {
            return await Container.GetService<BackupProcessor>().Process(-1, Request);
        }

        [HttpPost]
        [Route("setAcmeChallenge")]
        //[Microsoft.AspNetCore.Mvc.RequireHttps]
        [AllowAnonymous]
        //[Authorize] //Not sure if we can have authorization. should  be possible
        public async Task<IActionResult> SetAcmeChallenge()
        {
            //TODO:   1. Verify request using maybe key/value pair.
            //        2. Maybe just save to the correct path here with the correct file (maybe not, I can delete it using other way).
            try
            {
                var requestData = await WebsiteUtils.GetCurrentRequestData(HttpContextAccessor);
                
                //using (var stream = System.Web.HttpContext.Current.Request.InputStream)
                //using (var mem = new System.IO.MemoryStream())
                //{
                //    HttpContextAccessor.HttpContext.Request.Body.CopyTo(mem);
                //    //stream.CopyTo(mem);
                //    requestData = System.Text.Encoding.UTF8.GetString(mem.ToArray());
                //}

                Logger.Info("Set Acme Challenge Request data: " + requestData);

                var items = requestData.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var acmeString = items.Where(i => i.Contains("acme")).FirstOrDefault();
                var acmeValue = acmeString.Split('=').Last();

                Logger.Info("Acme value is: " + acmeValue);

                //TODO:   The below code should also work?
                //        And, should also be sending full path
                Logger.Info("Setting acme challenge");
                //Logger.Info("RequestData = " + requestData);
                //try
                //{
                //    var jData = JsonHelper.Parse(requestData);
                //    var acmeValueTmp = jData.GetValue("acme");
                //    Logger.Info("acme value was found using json parser");

                //    if (acmeValue == acmeValueTmp)
                //    {
                //        Logger.Info("Acme value is the same both ways");
                //    }
                //    else
                //    {
                //        Logger.Info("Acme challenge is not the same both ways");
                //    }
                //}
                //catch (Exception error)
                //{
                //    Logger.Info("Acme value not json: " + error.Message);
                //}

                var challengePath = "TODO";//

                AcmeController.ChallengeResponse = acmeValue;
                AcmeController.ChallengePath = challengePath;

                return new JsonResult("success: " + acmeValue)
                {
                    ContentType = "application/json"
                };//, JSON_SETTINGS);
            }
            catch (Exception error)
            {
                SystemLogger.LogError<MainController>("Error in setting acme challenge", error);
                return BadRequest("Unable to complete acme challenge: " + error.Message);
            }
        }
    }
}