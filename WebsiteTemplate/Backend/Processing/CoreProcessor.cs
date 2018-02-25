using log4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using Unity;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class CoreProcessor<T> : CoreProcessorBase
    {
        public CoreProcessor(IUnityContainer container)
            : base(container)
        {

        }

        protected static readonly ILog Logger = SystemLogger.GetLogger<T>();

        public abstract Task<T> ProcessEvent(int eventId);

        public async Task<IHttpActionResult> Process(int eventId, HttpRequestMessage requestMessage)
        {
            JsonResult<T> jsonResult;
            try
            {
                Logger.Debug("Process called for event id: " + eventId);
                await AuditService.LogUserEvent(eventId);
                var result = await ProcessEvent(eventId);

                if (result is FileActionResult)
                {
                    return result as FileActionResult;
                }

                jsonResult = new JsonResult<T>(result, JSON_SETTINGS, Encoding.UTF8, requestMessage);
                Logger.Debug("Result from processing " + eventId + " is:");
                Logger.Debug(new JavaScriptSerializer().Serialize(jsonResult.Content));
                return jsonResult;
            }
            catch (Exception error)
            {
                Logger.Error("Error in core processor during Process", error);

                return new BadRequestErrorMessageResult(error.Message + "\n" + error.StackTrace.ToString(), new DefaultContentNegotiator(), requestMessage, new List<MediaTypeFormatter>()
                {
                    new JsonMediaTypeFormatter()
                });
            }
        }
    }
}