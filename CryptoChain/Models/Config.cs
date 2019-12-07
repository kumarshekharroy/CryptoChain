using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Models
{
    public class Config
    {

        public static int HTTP_PORT { get { return 3000; } }
        public static DateTime JS_TIMESTAMP { get { return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); } }


        public static DateTime GENESIS_TIMESTAMP { get { return JS_TIMESTAMP; } } 
        public static string GENESIS_PREV_HASH { get { return string.Empty; } }
        public static long GENESIS_NONCE { get { return 0L; } }
        public static int GENESIS_DIFFICULTY { get { return 3; } }
        public static object GENESIS_DATA { get { return "AabraKaDabra"; } }




        public static int INITIAL_DIFFICULTY { get { return 3; } }

        public static int MINE_RATE_IN_MILLISEC { get { return 1000; } }


    }
}
