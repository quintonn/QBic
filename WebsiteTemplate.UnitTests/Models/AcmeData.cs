using Benoni.Core.Data.BaseTypes;
using Benoni.Core.Models;

namespace WebsiteTemplate.UnitTests.Models
{
    public class AcmeData : DynamicClass
    {
        public virtual string AccountName { get; set; }

        public virtual string AccountId { get; set; }

        public virtual LongString PrivateKey { get; set; }

        public virtual LongString PublicKey { get; set; }

        public virtual AcmeAccountType AccountType { get; set; }

        public virtual LongString CertificateKey { get; set; }

        public virtual string AccountEmail { get; set; }

        public virtual string TermsUrl { get; set; }

        public virtual string LocationUrl { get; set; }

        public virtual bool AgreedToTerms { get; set; }
    }

    public enum AcmeAccountType
    {
        Production = 0,
        Staging = 1
    }
}
