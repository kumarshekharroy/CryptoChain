using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
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
        private const bool encodeInHexValue = false;
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



        #region SerializeKeyPair
        public static (string pub, string priv) SerializeKeyPair(this AsymmetricCipherKeyPair pair, bool encodeInHex = encodeInHexValue)
        {
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(pair.Private);
            byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();
            string serializedPrivate = encodeInHex ? serializedPrivateBytes.ToHex() : Convert.ToBase64String(serializedPrivateBytes);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pair.Public);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            string serializedPublic = encodeInHex ? serializedPublicBytes.ToHex() : Convert.ToBase64String(serializedPublicBytes);
            return (serializedPublic, serializedPrivate);
        }

        public static string SerializePrivateKey(this AsymmetricKeyParameter privateKey, bool encodeInHex = encodeInHexValue)
        {
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();
            string serializedPrivate = encodeInHex ? serializedPrivateBytes.ToHex() : Convert.ToBase64String(serializedPrivateBytes);
            return serializedPrivate;

        }

        public static string SerializePublicKey(this AsymmetricKeyParameter publicKey, bool encodeInHex = encodeInHexValue)
        {
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            string serializedPublic = encodeInHex ? serializedPublicBytes.ToHex() : Convert.ToBase64String(serializedPublicBytes);
            return serializedPublic;
        }
        public static ECPrivateKeyParameters DeserializePrivateKey(string priv, bool encodeInHex = encodeInHexValue)
        {
            var keyByteArray = encodeInHex ? FromHex(priv) : Convert.FromBase64String(priv);
            ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(keyByteArray);
            return privateKey;
        }


        public static ECPublicKeyParameters DeserializePublicKey(string pub, bool encodeInHex = encodeInHexValue)
        {
            var keyByteArray = encodeInHex ? FromHex(pub) : Convert.FromBase64String(pub);
            ECPublicKeyParameters publicKey = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(keyByteArray);
            return publicKey;
        }
        #endregion




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


        public static string PrivateKey(this AsymmetricCipherKeyPair keypair, bool encodeInHex = encodeInHexValue) => encodeInHex ? ((keypair.Private as ECPrivateKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).D.ToByteArrayUnsigned().ToHex() : Convert.ToBase64String(((keypair.Private as ECPrivateKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).D.ToByteArrayUnsigned());

        public static string PublicKey(this AsymmetricCipherKeyPair keypair, bool encodeInHex = encodeInHexValue) => encodeInHex?((keypair.Public as ECPublicKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).Q.GetEncoded().ToHex() : Convert.ToBase64String(((keypair.Public as ECPublicKeyParameters) ?? throw new InvalidOperationException("invalid keypair.")).Q.GetEncoded());

        public static string ToEncodedString(this ECPrivateKeyParameters privateKey, bool encodeInHex = encodeInHexValue) => encodeInHex ? privateKey.D.ToByteArrayUnsigned().ToHex() : Convert.ToBase64String(privateKey.D.ToByteArrayUnsigned());

        public static string ToEncodedString(this ECPublicKeyParameters publicKey, bool encodeInHex = encodeInHexValue) => encodeInHex ? publicKey.Q.GetEncoded().ToHex() : Convert.ToBase64String(publicKey.Q.GetEncoded());
    }
}
