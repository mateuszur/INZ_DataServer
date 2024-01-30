using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Tls;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;

namespace DataServerGUI.Configurations
{
    public class ReadPFX
    {
        //public void CertificateReader(string pfxFilePath, string pfxPassword, Config config)
        //{
        //    try
        //    {
        //        // Wczytaj certyfikat PFX
        //        X509Certificate2 certificate = new X509Certificate2(pfxFilePath, pfxPassword, X509KeyStorageFlags.Exportable);
        //        ReadWriteConfig configSaver = new ReadWriteConfig();
                

        //        // Wyciągnij klucz publiczny
        //        var publicKey = certificate.PublicKey.Key.ToXmlString(false);
        //        MessageBox.Show("Klucz publiczny: " + publicKey);
        //        config.CertificatePublicKey = publicKey;
        //        configSaver.WriteConfiguration(config);

        //        // Wyciągnij klucz prywatny
        //        if (certificate.HasPrivateKey)
        //        {
        //            //    RSA rsa = RSA.Create();
        //            //    rsa = certificate.GetRSAPrivateKey();
        //            //    string privateKey = rsa.ExportRSAPrivateKey().ToString();
        //            //var privateKey = certificate.GetRSAPrivateKey().ToString();
        //            //var privateKey = certificate.PrivateKey;
        //            //
        //            //MessageBox.Show("Klucz prywatny: " + privateKey);
        //            //
        //            var rsaPrivateKey = certificate.GetRSAPrivateKey();

        //            var rsaParameters = rsaPrivateKey.ExportParameters(true);
        //            var bcPrivateKeyParameters = new RsaPrivateCrtKeyParameters(
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Modulus),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Exponent),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.D),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.P),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Q),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.DP),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.DQ),
        //                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.InverseQ));

        //            // Eksportuj klucz prywatny do formatu PEM
        //            using var stringWriter = new StringWriter();
        //            var pemWriter = new PemWriter(stringWriter);
        //            pemWriter.WriteObject(bcPrivateKeyParameters);
                   
        //            config.CertificatePrivateKey = stringWriter.ToString();
        //            configSaver.WriteConfiguration(config);


        //        }
        //        else
        //        {
        //            MessageBox.Show("Ten certyfikat nie zawiera klucza prywatnego.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Błąd odczytu certyfiaktu! \n"+ex.ToString());
        //    }
        //}


        public void CertificateReader2(string pfxFilePath, string pfxPassword)
        {
            try
            {// Wczytaj certyfikat PFX
                X509Certificate2 certificate = new X509Certificate2(pfxFilePath, pfxPassword, X509KeyStorageFlags.Exportable);


                // Wyciągnij klucz publiczny
                var publicKey = certificate.GetRSAPublicKey();
                if (publicKey == null)
                {
                    MessageBox.Show("Brak klucza publicznego! ");
                }
                else
                {

                  
                    using (TextWriter textWriter = new StreamWriter("..\\Config\\klucz_publiczny.pem"))
                    {
                        // Utwórz obiekt PemWriter
                        PemWriter pemWriter = new PemWriter(textWriter);

                        // Konwertuj klucz publiczny na obiekt Bouncy Castle
                        AsymmetricKeyParameter publicKeyParameter = DotNetUtilities.GetRsaPublicKey((RSA)publicKey);

                        // Zapisz klucz publiczny do pliku PEM
                        pemWriter.WriteObject(publicKeyParameter);
                    }
                }
                
                
            
                if (certificate.HasPrivateKey)
                {
                    // Wyodrębnij klucz prywatny
                    AsymmetricAlgorithm privateKey = certificate.GetRSAPrivateKey();

                    // Utwórz obiekt TextWriter do zapisu klucza prywatnego do pliku
                    using (TextWriter textWriter = new StreamWriter("..\\Config\\klucz_prywatny.pem"))
                    {
                        // Utwórz obiekt PemWriter
                        PemWriter pemWriter = new PemWriter(textWriter);

                        // Konwertuj klucz prywatny na obiekt Bouncy Castle
                        AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair((RSA)privateKey);

                        // Zapisz klucz prywatny do pliku PEM
                        pemWriter.WriteObject(keyPair.Private);
                    }

                }
                else
                {
                    MessageBox.Show("Ten certyfikat nie zawiera klucza prywatnego.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd odczytu certyfiaktu! \n" + ex.ToString());
            }
        }

    }
}
