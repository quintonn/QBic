using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;

namespace WebsiteTemplate.UnitTests.Models
{
    public class Department : DynamicClass
    {
        public virtual LongString Name { get; set; }
    }
}
