using SquishIt.Framework.Utilities;

namespace BuildHelper
{
    public class MyTrustLevel : ITrustLevel
    {
        public bool IsFullTrust
        {
            get
            {
                return true;
            }
        }

        public bool IsHighOrUnrestrictedTrust
        {
            get
            {
                return true;
            }
        }
    }
}
