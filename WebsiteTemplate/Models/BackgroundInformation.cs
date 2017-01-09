using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public class BackgroundInformation
    {
        public string Id { get; set; }

        public DateTime DateTimeUTC { get; set; }

        public string Task { get; set; }

        public string Information { get; set; }

        public BackgroundInformation()
        {
            Id = Guid.NewGuid().ToString();
        }

        public BackgroundInformation(string task, string information)
            :this()
        {
            DateTimeUTC = DateTime.UtcNow;
            Task = task;
            Information = information;
        }
    }
}