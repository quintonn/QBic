using Qactus.Authorization.Core;
using QBic.Core.Models;
using System;

namespace WebsiteTemplate.Models
{
    public class RefreshToken : DynamicClass, IRefreshToken
    {
        public RefreshToken()
        {
            Id = Guid.NewGuid().ToString();
    }
        public virtual string Token { get; set; }
        //[Required]
        //[MaxLength(50)]
        //public string Subject { get; set; }

        //[Required]
        //[MaxLength(50)]
        //public string ClientId { get; set; }

        //public DateTime IssuedUtc { get; set; }

        //public DateTime ExpiresUtc { get; set; }

        //[Required]
        //public string ProtectedTicket { get; set; }
    }
}
