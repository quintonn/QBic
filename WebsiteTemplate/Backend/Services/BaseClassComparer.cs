using System.Collections.Generic;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class BaseClassComparer : IEqualityComparer<BaseClass>
    {
        public bool Equals(BaseClass x, BaseClass y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(BaseClass obj)
        {
            return 1;
        }
    }
}