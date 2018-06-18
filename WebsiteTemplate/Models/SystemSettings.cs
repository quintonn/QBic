using QCumber.Core.Models;

namespace WebsiteTemplate.Models
{
    public class SystemSettings : DynamicClass
    {
        public virtual string EmailFromAddress { get; set; }

        public virtual string EmailHost { get; set; }

        public virtual string EmailUserName { get; set; }

        public virtual string EmailPassword { get; set; }

        public virtual int EmailPort { get; set; }

        public virtual bool EmailEnableSsl { get; set; }

        public virtual string DateFormat { get; set; }

        public virtual int TimeOffset { get; set; }

        public virtual string WebsiteBaseUrl { get; set; }

        public SystemSettings()
        {
            DateFormat = "dd-MM-yyyy";
        }
    }
}