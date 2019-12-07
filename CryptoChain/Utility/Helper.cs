using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoChain.Utility
{
    public class Helper
    {
        public static string Sha256(params string[] values)
        {

            using var hash = System.Security.Cryptography.SHA256.Create();

            Array.Sort(values);

            Byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(string.Join(string.Empty, values)));

            StringBuilder Sb = new StringBuilder(64);

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));

            return Sb.ToString();
        }
        public static string Sha256(string values)
        {

            using var hash = System.Security.Cryptography.SHA256.Create();

            Byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(values));

            StringBuilder Sb = new StringBuilder(64);

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));

            return Sb.ToString();
        }
        public static string Hex2Binary(string hexvalue)
        {
            // Convert.ToUInt32 this is an unsigned int
            // so no negative numbers but it gives you one more bit
            // it much of a muchness 
            // Uint MAX is 4,294,967,295 and MIN is 0
            // this padds to 4 bits so 0x5 = "0101"
            return string.Join(string.Empty, hexvalue.Select(c => Convert.ToString(Convert.ToUInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }
    }
}
