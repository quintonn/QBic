using Microsoft.AspNetCore.Identity;

namespace QBic.Authentication
{
    class DefaultLookupNormalizer : ILookupNormalizer
    {
        public string NormalizeEmail(string email)
        {
            return email?.ToLower();
        }

        public string NormalizeName(string name)
        {
            return name;//?;.ToLower();
        }
    }
}
