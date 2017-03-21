using System;

namespace WebsiteTemplate.Models
{
    public class BackgroundInformation : DynamicClass
    {
        //public string Id { get; set; }

        public virtual DateTime DateTimeUTC { get; set; }

        public virtual string Task { get; set; }

        public virtual string Information { get; set; }

        public BackgroundInformation()
        {
            //Id = Guid.NewGuid().ToString();
        }

        public BackgroundInformation(string task, string information)
        //:this()
        {
            DateTimeUTC = DateTime.UtcNow;
            Task = task;
            Information = information;
        }
    }
}