using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaFling
{
    [Flags]
    enum Token
    {
        access = 1,
        access_secret = 2,
        oauth = 4,
        oauth_secret = 8,
        verifier = 16,
        callback = 32
    }
}
