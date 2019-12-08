using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
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
                secureRandom.SetSeed(new ASCIIEncoding().GetBytes(seed));

            var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(keyParams);
            var keyPair = generator.GenerateKeyPair();

            //var privateKey = keyPair.Private as ECPrivateKeyParameters;
            //var publicKey = keyPair.Public as ECPublicKeyParameters;

            //Console.WriteLine($"Private key: {ToHex(privateKey.D.ToByteArrayUnsigned())}");
            //Console.WriteLine($"Public key: {ToHex(publicKey.Q.GetEncoded())}");

            return keyPair;
        }
        //public static bool VerifySignature(this AsymmetricCipherKeyPair key, string data, string signature)
        //{
        //    var inputData = Encoding.UTF8.GetBytes(Helper.Sha256(data));
        //    var signer = SignerUtilities.GetSigner("secp256k1");
        //    signer.Init(false, key.Public);
        //    signer.BlockUpdate(inputData, 0, inputData.Length);
        //    return signer.VerifySignature(Convert.FromBase64String(signature));
        //}
        public static bool VerifySignature(string publicKey, string data, string signature)
        { 
            var inputData = Encoding.UTF8.GetBytes(Helper.Sha256(data));
            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(false, (AsymmetricKeyParameter)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey)));
            signer.BlockUpdate(inputData, 0, inputData.Length);
            return signer.VerifySignature(Convert.FromBase64String(signature));
        }
        public static string Sign(this AsymmetricCipherKeyPair keyPair, string data)
        { 
            var inputData = Encoding.UTF8.GetBytes(Helper.Sha256(data)); 
            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(true,keyPair.Private);
            signer.BlockUpdate(inputData, 0, inputData.Length);
             
            return Convert.ToBase64String(signer.GenerateSignature());

        }


        static string ToHex(this byte[] data) => String.Concat(data.Select(x => x.ToString("x2")));


        public static string PrivateKey(this AsymmetricCipherKeyPair keypair) => ((keypair.Private as ECPrivateKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).D.ToByteArrayUnsigned().ToHex();

        public static string PublicKey(this AsymmetricCipherKeyPair keypair) => ((keypair.Public as ECPublicKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).Q.GetEncoded().ToHex();
    }
}
