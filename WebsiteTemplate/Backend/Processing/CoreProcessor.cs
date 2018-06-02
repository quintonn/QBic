using Benoni.Core.Utilities;
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

        public abstract Task<T> ProcessEvent(int eventId);

        public async Task<IHttpActionResult> Process(int eventId, HttpRequestMessage requestMessage)
        {
            JsonResult<T> jsonResult;
            try
            {
                await AuditService.LogUserEvent(eventId);
                var result = await ProcessEvent(eventId);

                if (result is FileActionResult)
                {
                    return result as FileActionResult;
                }

                jsonResult = new JsonResult<T>(result, JSON_SETTINGS, Encoding.UTF8, requestMessage);
                //Logger.Debug("Result from processing " + eventId + " is:");
                //Logger.Debug(new JavaScriptSerializer().Serialize(jsonResult.Content));
                return jsonResult;
            }
            catch (Exception error)
            {
                SystemLogger.LogError("Error in core processor during Process", this.GetType(), error);

                return new BadRequestErrorMessageResult(error.Message + "\n" + error.StackTrace.ToString(), new DefaultContentNegotiator(), requestMessage, new List<MediaTypeFormatter>()
                {
                    new JsonMediaTypeFormatter()
                });
            }
        }
    }
}