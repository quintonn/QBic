using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Models
{
    public class AuditEvent : BaseClass
    {
        public virtual User User { get; set; }
        public virtual DateTime AuditEventDateTimeUTC { get; set; }
        public virtual AuditAction AuditAction { get; set; }
        public virtual string ObjectId { get; set; }
        public virtual string EntityName { get; set; }
        public virtual string OriginalObject { get; set; }
        public virtual string NewObject { get; set; }

        //public IList<EventRoleAssociation> EventRoles { get; set; }
    }
}