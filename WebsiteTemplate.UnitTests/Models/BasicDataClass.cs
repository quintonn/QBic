using QCumber.Core.Data.BaseTypes;
using QCumber.Core.Models;

namespace WebsiteTemplate.UnitTests.Models
{
    public class BasicDataClass : DynamicClass
    {
        public virtual string Text { get; set; }

        public virtual int Number { get; set; }

        public virtual LongString LongText { get; set; }
    }
}
