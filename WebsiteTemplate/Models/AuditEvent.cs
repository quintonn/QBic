using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public class AuditEvent : BaseClass
    {
        public User User { get; set; }
        public DateTime AuditEventDateTimeUTC { get; set; }
        public AuditAction AuditAction { get; set; }
        public string ObjectId { get; set; }
        public string EntityName { get; set; }
        public string OriginalObject { get; set; }
        public string NewObject { get; set; }
    }
}