using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public enum Comparison
    {
        Equals = 0,
        NotEquals = 1,
        Contains = 2,
        IsNotNull = 3,
        IsNull = 4,
        GreaterThan = 5,
        GreaterThanOrEqual = 6,
        LessThan = 7,
        LessThanOrEqual = 8
    }
}