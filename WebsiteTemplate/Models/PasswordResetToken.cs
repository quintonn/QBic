using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;
using System;

namespace WebsiteTemplate.Models
{
    public class PasswordResetToken : DynamicClass
    {
        public virtual LongString Token { get; set; }
        public virtual DateTime Expiration { get; set; }
    }
}
