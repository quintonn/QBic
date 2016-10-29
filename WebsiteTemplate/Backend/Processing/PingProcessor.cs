﻿using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class PingProcessor : EventProcessor<PingResult>
    {
        public PingProcessor(IUnityContainer container)
            : base(container)
        {

        }

        public override async Task<PingResult> ProcessEvent(int eventId)
        {
            var data = GetRequestData();
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