﻿using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public abstract class FilterItem : DynamicClass
    {
        public virtual LongString FilterValue { get; set; }

        public virtual FilterItemParentBase Parent { get; set; }
    }
}