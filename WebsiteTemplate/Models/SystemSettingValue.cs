using QCumber.Core.Models;

namespace WebsiteTemplate.Models
{
    public class SystemSettingValue : DynamicClass
    {
        public virtual string KeyName { get; set; }

        public virtual string Value { get; set; }
    }
}