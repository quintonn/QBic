using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QBic.Core.Utilities;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebsiteTemplate.Controllers;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class CoreProcessor<T> : CoreProcessorBase
    {
        public CoreProcessor(IServiceProvider container)
            : base(container)
        {

        }

        public abstract Task<T> ProcessEvent(int eventId);

        public async Task<IActionResult> Process(int eventId, HttpRequest requestMessage)
        {
            OkObjectResult jsonResult;
            //JsonResult<T> jsonResult;
            try
            {
                await AuditService.LogUserEvent(eventId);
                var result = await ProcessEvent(eventId);

                if (result is FileActionResult)
                {
                    return result as FileActionResult;
                }

                if (result is FileContentResult)
                {
                    return result as FileContentResult;
                }

                //jsonResult = new JsonResult<T>(result, JSON_SETTINGS, Encoding.UTF8, requestMessage);
                jsonResult = new OkObjectResult (result);
                //{
                //    ContentType = "application/json",
                //};
                jsonResult.ContentTypes.Add("application/json");
                
                //Logger.Debug("Result from processing " + eventId + " is:");
                //Logger.Debug(new JavaScriptSerializer().Serialize(jsonResult.Content));
                return jsonResult;
            }
            catch (Exception error)
            {
                SystemLogger.LogError("Error in core processor during Process", this.GetType(), error);
                return new BadRequestObjectResult(JsonSerializer.Serialize(SystemLogger.GetMessageStack(error)));
                //return new BadRequestErrorMessageResult(SystemLogger.GetMessageStack(error), new DefaultContentNegotiator(), requestMessage, new List<MediaTypeFormatter>()
                //{
                //    new JsonMediaTypeFormatter()
                //});
            }
        }
    }
}