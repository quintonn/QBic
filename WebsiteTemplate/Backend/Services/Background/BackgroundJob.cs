﻿using System;
using WebsiteTemplate.Menus;

namespace WebsiteTemplate.Backend.Services.Background
{
    public class BackgroundJob
    {
        public BackgroundEvent Event { get; set; }

        public int EventNumber { get; set; }

        public DateTime? LastRunTime { get; set; }

        public DateTime NextRunTime { get; set; }

        public bool WillRunNext { get; set; }
    }
}