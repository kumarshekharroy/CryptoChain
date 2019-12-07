using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Models
{
    public class Config
    {

        public static int INITIAL_DIFFICULTY { get { return 3; } }

        public static int MINE_RATE_IN_MILLISEC { get { return 1000; } }

    }
}
