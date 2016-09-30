using System;
using WebsiteTemplate.Data.BaseTypes;

namespace WebsiteTemplate.Models
{
    public class BackgroundJobResult : DynamicClass
    {
        public virtual int EventNumber { get; set; }

        public virtual DateTime? DateTimeRunUTC { get; set; }

        public virtual string Status { get; set; }

        public virtual LongString ExecutionInformation { get; set; }
    }
}