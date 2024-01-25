using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using Org.BouncyCastle.OpenSsl;

namespace DataServerGUI.Configurations
{
    public class ReadPFX
    {
        public void CertificateReader(string pfxFilePath, string pfxPassword, Config config)
        {
            try
            {
                // Wczytaj certyfikat PFX
                X509Certificate2 certificate = new X509Certificate2(pfxFilePath, pfxPassword, X509KeyStorageFlags.Exportable);
                ReadWriteConfig configSaver = new ReadWriteConfig();
                

                // Wyciągnij klucz publiczny
                var publicKey = certificate.PublicKey.Key.ToXmlString(false);
                MessageBox.Show("Klucz publiczny: " + publicKey);
                config.CertificatePublicKey = publicKey;
                configSaver.WriteConfiguration(config);

                // Wyciągnij klucz prywatny
                if (certificate.HasPrivateKey)
                {
                    //    RSA rsa = RSA.Create();
                    //    rsa = certificate.GetRSAPrivateKey();
                    //    string privateKey = rsa.ExportRSAPrivateKey().ToString();
                    //var privateKey = certificate.GetRSAPrivateKey().ToString();
                    //var privateKey = certificate.PrivateKey;
                    //
                    //MessageBox.Show("Klucz prywatny: " + privateKey);
                    //
                    var rsaPrivateKey = certificate.GetRSAPrivateKey();

                    var rsaParameters = rsaPrivateKey.ExportParameters(true);
                    var bcPrivateKeyParameters = new RsaPrivateCrtKeyParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Modulus),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Exponent),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.D),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.P),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Q),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.DP),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.DQ),
                        new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.InverseQ));

                    // Eksportuj klucz prywatny do formatu PEM
                    using var stringWriter = new StringWriter();
                    var pemWriter = new PemWriter(stringWriter);
                    pemWriter.WriteObject(bcPrivateKeyParameters);
                   
                    config.CertificatePrivateKey = stringWriter.ToString();
                    configSaver.WriteConfiguration(config);


                }
                else
                {
                    MessageBox.Show("Ten certyfikat nie zawiera klucza prywatnego.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd odczytu certyfiaktu! \n"+ex.ToString());
            }
        }
    }
}
