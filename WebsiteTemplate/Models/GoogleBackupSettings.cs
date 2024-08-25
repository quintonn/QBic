using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;

namespace WebsiteTemplate.Models
{
    public class GoogleBackupSettings : DynamicClass
    {
        public virtual string ApplicationName { get; set; }
        public virtual string ParentFolder { get; set; }
        public virtual byte[] CredentialJsonValue { get; set; }
    }
}
