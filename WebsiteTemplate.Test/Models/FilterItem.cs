using WebsiteTemplate.Data.BaseTypes;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Test.Models
{
    public abstract class FilterItem : DynamicClass
    {
        //public virtual Question Question { get; set; }

        //public virtual FilterComparison Comparison { get; set; }

        public virtual LongString FilterValue { get; set; }

        public virtual FilterItemParentBase Parent { get; set; }
    }
}