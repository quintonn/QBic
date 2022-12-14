using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public class FileItem : DynamicClass
    {
        public virtual string FileName { get; set; }
        public virtual string FileExtension { get; set; }
        public virtual string MimeType { get; set; }
        public virtual byte[] FileData { get; set; }
        public virtual MarketplaceItem MarketplaceItem { get; set; }
    }
}
