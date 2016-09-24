using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;

namespace WebsiteTemplate.Backend.Services
{
    public class BackgroundJob
    {
        public BackgroundEvent Event { get; set; }

        public DateTime? LastRunTime { get; set; }

        public DateTime NextRunTime { get; set; }

        public bool WillRunNext { get; set; }
    }
}