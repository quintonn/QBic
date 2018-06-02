using Benoni.Core.Data.BaseTypes;
using Benoni.Core.Models;
using System;

namespace WebsiteTemplate.Models
{
    public class BackgroundInformation : DynamicClass
    {
        public virtual DateTime DateTimeUTC { get; set; }

        public virtual string Task { get; set; }

        public virtual LongString Information { get; set; }

        public BackgroundInformation()
        {

        }

        public BackgroundInformation(string task, string information)
        {
            DateTimeUTC = DateTime.UtcNow;
            Task = task;
            Information = information;
        }
    }
}