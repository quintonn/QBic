using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class PingProcessor : EventProcessor<PingResult>
    {
        public PingProcessor(IServiceProvider container, ILogger<PingProcessor> logger)
            : base(container, logger)
        {

        }

        public override async Task<PingResult> ProcessEvent(int eventId)
        {
            var data = await GetRequestData();
            var json = JsonHelper.Parse(data);

            return new PingResult("good job");
        }
    }

    public class PingResult
    {
        public string Message { get; set; }

        public PingResult(string message)
        {
            Message = message;
        }
    }
}