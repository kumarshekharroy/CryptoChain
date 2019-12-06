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

    }
}
