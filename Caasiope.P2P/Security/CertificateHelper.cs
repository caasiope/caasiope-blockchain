using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Caasiope.P2P.Security
{
    public class CertificateHelper
    {
        const int KEY_STRENGTH = 2048;
        const string SIGNATURE_ALGORYTHM = "SHA256WithRSA";

        // https://stackoverflow.com/questions/22230745/generate-self-signed-certificate-on-the-fly
        public static X509Certificate2 GenerateCertificate(string filename)
        {
            // TODO subject and issue as parameters
            AsymmetricKeyParameter caPrivateKey = null;
            var caCert = GenerateCACertificate("CN=MyROOTCA", ref caPrivateKey);
            // addCertToStore(caCert, StoreName.Root, StoreLocation.LocalMachine);

            var clientCert = GenerateSelfSignedCertificate("CN=127.0.0.1", "CN=MyROOTCA", caPrivateKey);

            ExportCertificate(clientCert, filename);
            // addCertToStore(new X509Certificate2(p12, (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet), StoreName.My, StoreLocation.LocalMachine);
            /*
            var cert = new X509Certificate2(File.ReadAllBytes(filename), String.Empty);



            X509Chain chain = new X509Chain();

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            var isOk = chain.Build(cert);
            if (chain.ChainStatus.Length != 0)
            {
                foreach (X509ChainStatus objChainStatus in chain.ChainStatus)
                {
                    var error = objChainStatus.Status.ToString() + " - " + objChainStatus.StatusInformation;
                }
            }
            */
            return clientCert;
        }

        public static void ExportCertificate(X509Certificate2 certificate, string filename)
        {
            var bytes = certificate.Export(X509ContentType.Pfx);
            File.WriteAllBytes(filename, bytes);
        }

        public static X509Certificate2 LoadCertificate(string filename)
        {
            var cert = new X509Certificate2(File.ReadAllBytes(filename), String.Empty, X509KeyStorageFlags.Exportable); // TODO not safe but usefull for signing
            return cert;
        }

        private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName, AsymmetricKeyParameter issuerPrivKey)
        {
            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Signature Algorithm
            certificateGenerator.SetSignatureAlgorithm(SIGNATURE_ALGORYTHM);

            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = new X509Name(issuerName);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            var keyGenerationParameters = new KeyGenerationParameters(random, KEY_STRENGTH);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;

            // selfsign certificate
            var certificate = certificateGenerator.Generate(issuerPrivKey, random);
            
            var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());
            // merge into X509Certificate2
            MergePrivateKey(x509, subjectKeyPair.Private);

            return x509;

        }

        private static AsymmetricAlgorithm ToDotNetKey(RsaPrivateCrtKeyParameters privateKey)
        {
            var cspParams = new CspParameters
            {
                KeyContainerName = Guid.NewGuid().ToString(),
                KeyNumber = (int)KeyNumber.Exchange,
                Flags = CspProviderFlags.UseMachineKeyStore
            };

            var rsaProvider = new RSACryptoServiceProvider(cspParams);
            var parameters = new RSAParameters
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned()
            };

            rsaProvider.ImportParameters(parameters);
            return rsaProvider;
        }

        public static X509Certificate2 GenerateCACertificate(string subjectName, ref AsymmetricKeyParameter CaPrivateKey)
        {
            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);
            
            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, KEY_STRENGTH);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;

            // selfsign certificate
            var certificate = certificateGenerator.Generate(new Asn1SignatureFactory(SIGNATURE_ALGORYTHM, issuerKeyPair.Private, random));
            var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());

            CaPrivateKey = issuerKeyPair.Private;

            return x509;
            //return issuerKeyPair.Private;
        }
        
        public static X509Certificate2 GenerateSignedCertificate(string subject, AsymmetricKeyParameter publickey, X509Certificate2 authority)
        {

            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            var subjectDN = new X509Name(subject);
            var issuerDN = new X509Name(authority.Subject);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            var subjectKeyPair = publickey; //DotNetUtilities.GetRsaPublicKey((RSACryptoServiceProvider) signee.PublicKey.Key);
            certificateGenerator.SetPublicKey(subjectKeyPair);

            var akp = TransformRSAPrivateKey(authority.PrivateKey);
            
            // Signature Algorithm
            var certificate = certificateGenerator.Generate(new Asn1SignatureFactory(SIGNATURE_ALGORYTHM, akp, random));
            var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());

            return x509;
        }

        private static bool addCertToStore(System.Security.Cryptography.X509Certificates.X509Certificate2 cert, System.Security.Cryptography.X509Certificates.StoreName st, System.Security.Cryptography.X509Certificates.StoreLocation sl)
        {
            var bRet = false;

            try
            {
                var store = new X509Store(st, sl);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
            }
            catch
            {

            }

            return bRet;
        }

        // https://stackoverflow.com/questions/3240222/get-private-key-from-bouncycastle-x509-certificate-c-sharp
        public static AsymmetricKeyParameter TransformRSAPrivateKey(AsymmetricAlgorithm privateKey)
        {
            var prov = privateKey as RSACryptoServiceProvider;
            var parameters = prov.ExportParameters(true);

            return new RsaPrivateCrtKeyParameters(
                new BigInteger(1, parameters.Modulus),
                new BigInteger(1, parameters.Exponent),
                new BigInteger(1, parameters.D),
                new BigInteger(1, parameters.P),
                new BigInteger(1, parameters.Q),
                new BigInteger(1, parameters.DP),
                new BigInteger(1, parameters.DQ),
                new BigInteger(1, parameters.InverseQ));
        }


        // https://stackoverflow.com/questions/6497040/how-do-i-validate-that-a-certificate-was-created-by-a-particular-certification-a
        public static bool IsCertificateSignedByAuthority(X509Certificate2 authority, X509Certificate2 certificate)
        {
            var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            chain.ChainPolicy.VerificationTime = DateTime.Now;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);

            // This part is very important. You're adding your known root here.
            // It doesn't have to be in the computer store at all. Neither certificates do.
            chain.ChainPolicy.ExtraStore.Add(authority);

            // TODO still the same problem, can't validate chain because we are self signed
            var isChainValid = chain.Build(certificate);

            /*
            if (!isChainValid)
            {
                // Trust chain did not complete to the known authority anchor
                return false;
            }
            */

            // This piece makes sure it actually matches your known root
            var isValid = chain.ChainElements
                .Cast<X509ChainElement>()
                .Any(x => x.Certificate.Thumbprint == authority.Thumbprint);

            return isValid;
        }

        public static void ExportPublicKey(X509Certificate2 certificate, string path)
        {
            File.WriteAllText(path, certificate.PublicKey.Key.ToXmlString(false));
        }

        public static RsaKeyParameters ImportPublicKey(string path)
        {
            var provider = new RSACryptoServiceProvider();
            provider.FromXmlString(File.ReadAllText(path));
            if (!provider.PublicOnly)
                throw new ArgumentException("Please provide the path of the xml file that contains only the public key");
            return DotNetUtilities.GetRsaPublicKey(provider);
        }

        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            var keyGenerationParameters = new KeyGenerationParameters(random, KEY_STRENGTH);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }

        public static void MergePrivateKey(X509Certificate2 certificate, AsymmetricKeyParameter keyPrivate)
        {
            // correcponding private key
            var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPrivate);

            var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
            if (seq.Count != 9)
            {
                throw new PemException("malformed sequence in RSA private key");
            }

            var rsa = RsaPrivateKeyStructure.GetInstance(seq);
            var rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

            certificate.PrivateKey = ToDotNetKey(rsaparams); //x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
        }
    }
}
