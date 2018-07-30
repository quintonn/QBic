using QBic.Core.Models;
using System;

namespace WebsiteTemplate.Models
{
    public class AuditEvent : BaseClass
    {
        public virtual string UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual DateTime AuditEventDateTimeUTC { get; set; }
        public virtual AuditAction AuditAction { get; set; }
        public virtual string ObjectId { get; set; }
        public virtual string EntityName { get; set; }
        public virtual string OriginalObject { get; set; }
        public virtual string NewObject { get; set; }

        //public IList<EventRoleAssociation> EventRoles { get; set; }
    }
}