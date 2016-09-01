using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Results;
using WebsiteTemplate.Controllers;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class CoreProcessor<T> : CoreProcessorBase
    {
        public CoreProcessor(IUnityContainer container)
            : base(container)
        {

        }
        public abstract Task<T> ProcessEvent(int eventId);

        internal async Task<IHttpActionResult> Process(int eventId, HttpRequestMessage requestMessage)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    await AuditService.LogUserEvent(eventId);
                    var result = await ProcessEvent(eventId);

                    if (result is FileActionResult)
                    {
                        return result as FileActionResult;
                    }

                    var jsonResult = new JsonResult<T>(result, JSON_SETTINGS, Encoding.UTF8, requestMessage);
                    scope.Complete();
                    return jsonResult;
                }
                catch (Exception error)
                {
                    return new BadRequestErrorMessageResult(error.Message, new DefaultContentNegotiator(), requestMessage, new List<MediaTypeFormatter>()
                    {
                        new JsonMediaTypeFormatter()
                    });
                }
            }
        }
    }
}