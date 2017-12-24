using SquishIt.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
