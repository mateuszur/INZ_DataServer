using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DataServerGUI.Configurations
{
    public class ReadWriteConfig
    {
        public void ReadConfiguration(Config config)
        {
            try
            {
                
                var configLines = File.ReadAllLines("..\\Config\\ServerConfig.txt");
                foreach (var line in configLines)
                {
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "DataServerPort":
                                config.DataServerPort = int.Parse(keyValue[1]);
                                break;
                            case "FTPServerPort":
                                config.FTPServerPort = keyValue[1];
                                break;
                            case "FTPUsername":
                                config.FTPUsername = keyValue[1];
                                break;
                            case "FTPPassword":
                                config.FTPPassword = keyValue[1];
                                break;
                            
                            case "FilePath":
                                config.FilePath = keyValue[1];
                                break;

                            case "Key":
                                config.Key = keyValue[1];
                                break;
                            
                            case "IV":
                                    config.IV = keyValue[1];
                                break;
                        }
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show("Błąd podczas odczytu pliku konfiguracyjnego!");
            }
        }

        public void WriteConfiguration(Config config)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("..\\Config\\ServerConfig.txt"))
                {
                    sw.WriteLine($"DataServerPort={config.DataServerPort}");
                    sw.WriteLine($"FTPServerPort={config.FTPServerPort}");
                    sw.WriteLine($"FTPUsername={config.FTPUsername}");
                    sw.WriteLine($"FTPPassword={config.FTPPassword}");
                    sw.WriteLine($"FilePath={config.FilePath}");
                    sw.WriteLine($"Key={config.Key}");
                    sw.WriteLine($"IV={config.IV}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas zapisu pliku konfiguracyjnego!");
            }
        }

        public void WriteConfigurationClient(ClientConfig config)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("..\\Config\\config.txt"))
                {
                    sw.WriteLine($"ServerAddress={config.ServerAddress}");
                    sw.WriteLine($"DataServerPort={config.DataServerPort}");
                    sw.WriteLine($"SFTPPort={config.SFTPPort}");
                    sw.WriteLine($"Key={config.Key}");
                    sw.WriteLine($"IV={config.IV}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas zapisu pliku konfiguracyjnego!");
            }
        }

    }
}
