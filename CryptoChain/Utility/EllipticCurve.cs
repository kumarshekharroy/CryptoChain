using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoChain.Utility
{
    public static class EllipticCurve
    {
        public static AsymmetricCipherKeyPair GenerateKeys(string seed = "", int keySize = 256)
        { 
            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var secureRandom = new SecureRandom();

            if (!string.IsNullOrWhiteSpace(seed))
                secureRandom.SetSeed(Encoding.UTF8.GetBytes(seed));

            var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(keyParams);
            var keyPair = generator.GenerateKeyPair();
            return keyPair;
        }
        public static bool VerifySignature(string publicKey, string data, string signature)
        {
            var inputData = Encoding.UTF8.GetBytes(Helper.Sha256(data));

            X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
            ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(false, new ECPublicKeyParameters("ECDSA", curve.Curve.DecodePoint(FromHex(publicKey)), curveSpec));
            signer.BlockUpdate(inputData, 0, inputData.Length);
            return signer.VerifySignature(FromHex(signature));
        }
        public static string Sign(this AsymmetricCipherKeyPair keyPair, string data)
        {
            var inputData = Encoding.UTF8.GetBytes(Helper.Sha256(data));
            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(true, keyPair.Private);
            signer.BlockUpdate(inputData, 0, inputData.Length);

            return signer.GenerateSignature().ToHex();

        }


        static string ToHex(this byte[] data) => String.Concat(data.Select(x => x.ToString("x2")));

        static byte[] FromHex(string data) => Enumerable.Range(0, data.Length / 2).Select(x => Convert.ToByte(data.Substring(2 * x, 2), 16)).ToArray();
        //{
        //    byte[] bytes = new byte[data.Length / 2];
        //    for (int i = 0; i < data.Length / 2; i++)
        //    {
        //        bytes[i] = Convert.ToByte(data.Substring(2 * i, 2), 16);
        //    }
        //    ;
        //    return bytes;
        //}


        public static string PrivateKey(this AsymmetricCipherKeyPair keypair) => ((keypair.Private as ECPrivateKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).D.ToByteArrayUnsigned().ToHex();

        public static string PublicKey(this AsymmetricCipherKeyPair keypair) => ((keypair.Public as ECPublicKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).Q.GetEncoded().ToHex();
    }
}
