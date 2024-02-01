using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DataServerGUI.Configurations;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text;
using MySqlX.XDevAPI;
using System.Net.Sockets;
using System.Security.Cryptography;


namespace Server
{


    public partial class MainMenu : Window
    {
        private int test;
        private int user_privilege;
        private string username;
        private string serverAddress = "";
        private string dbAddress = "";
        private string usernameFTP = "";
        private string passwordFTP = "";
        private string portFTP = "";
        private string inpuCert = "";
        private string passwordCert;
        private string inputPath = "";

        // konfiguracja do zaczytania z pliku
        private int dataServerPort = 0;
        private string ftpServerPort = "";
        private string ftpUsername = "";
        private string ftpPassword = "";
        private string filePath = "";

        private byte[] key;
        private byte[] iv;

        ReadWriteConfig configReadWrite = new ReadWriteConfig();
        Config config = new Config();

        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();



        public MainMenu(int user_privilege, string username)
        {
            InitializeComponent();
            connection_string = fileManager.ReadParameter();
            this.user_privilege = user_privilege;
            this.username = username;
            configReadWrite.ReadConfiguration(config);
            this.dataServerPort = config.DataServerPort;
            this.ftpServerPort = config.FTPServerPort;
            this.ftpUsername = config.FTPUsername;
            this.ftpPassword = config.FTPPassword;
            this.filePath = config.FilePath;
            this.key = StringToByteArray(config.Key);
            this.iv = StringToByteArray(config.IV);
        }

        private void Server_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            ChangePageVisibility(Server_Setting_Content);
        }





        private void Server_Users_Button_Click(object sender, RoutedEventArgs e)
        {
            ChangePageVisibility(Server_Users_Content);

            connection_name.ConnectionString = connection_string;

            try
            {
                if (user_privilege == 1)
                {
                    connection_name.Open();

                    string querry = "SELECT ID, Login, Privileges, Name, Surname, Space_available, Disk_space_used FROM `View_Users`;";

                    MySqlCommand commend = new MySqlCommand(querry, connection_name);
                    MySqlDataReader data_from_querry = commend.ExecuteReader();

                    var listOfUsers = new List<Entry_Users>();


                    while (data_from_querry.Read())
                    {

                        Entry_Users feld = new Entry_Users
                        {
                            ID = data_from_querry.GetInt32(0),
                            Login = data_from_querry.GetString(1),
                            Privileges = data_from_querry.GetInt32(2).ToString(),
                            Name = data_from_querry.GetString(3),
                            Surname = data_from_querry.GetString(4),
                            Space_available = Math.Round(data_from_querry.GetDouble(5) / 1024, 2),

                            Disk_space_used = Math.Round(data_from_querry.GetDouble(6) / 1024, 2)
                        };


                        listOfUsers.Add(feld);
                    }

                    Users_Data_Grid.ItemsSource = listOfUsers;
                    connection_name.Close();
                }
                else
                {
                    MessageBox.Show("Brak uprawnień!");
                }
            }
            catch (Exception ex)
            {
                connection_name?.Close();
                MessageBox.Show("Otwieranie zarządzania użytkownikami... \n" + ex);
            }


        }

        private void ChangePageVisibility(Grid pageToShow)
        {
            Server_Setting_Content.Visibility = Visibility.Collapsed;
            Server_Users_Content.Visibility = Visibility.Collapsed;

            pageToShow.Visibility = Visibility.Visible;
        }


        private void Server_Logout(object sender, EventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();

        }

        private void SelectCertificateFile(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    InitialDirectory = "c:\\Donwload",
                    Filter = "PFX files (*.pfx)|*.pfx",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    inpuCert = dialog.FileName;

                    CertificatePath.TextWrapping = TextWrapping.WrapWithOverflow;
                    CertificatePath.Text = inpuCert;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Probem z dostępem do plików!.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SelectUserPath(object sender, EventArgs e)
        {
            try
            {
                var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
                {
                    InitialDirectory = "c:\\Download",
                    IsFolderPicker = true
                };

                if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
                    inputPath = dialog.FileName;

                    UserPath.TextWrapping = TextWrapping.WrapWithOverflow;
                    UserPath.Text = inputPath;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Probem z dostępem do plików!.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Save_Server_Config(object sender, RoutedEventArgs e)
        {
            try
            {
                serverAddress = ServerAddress.Text;
                dbAddress = DBAddress.Text;
                usernameFTP = FTPUsername.Text;
                passwordFTP = FTPPassword.Password;
                portFTP = SFTPPort.Text;
                passwordCert = CertificatePass.Password;

                if (AddressValidation(serverAddress) == "")
                {
                    ServerAddress.BorderBrush = Brushes.Red; ServerAddress.Focus();
                    return;

                }

                if (AddressValidation(dbAddress) == "")
                {
                    DBAddress.BorderBrush = Brushes.Red; DBAddress.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(usernameFTP))
                {
                    FTPUsername.BorderBrush = Brushes.Red; FTPUsername.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(passwordFTP))
                {
                    FTPPassword.BorderBrush = Brushes.Red; FTPPassword.Focus();
                    return;
                }

                if (SFTPPortValidation(portFTP) == "")
                {
                    FTPPassword.BorderBrush = Brushes.Red; FTPPassword.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(passwordCert))
                {
                    CertificatePass.BorderBrush = Brushes.Red; CertificatePass.Focus();

                    return;
                }

                if (inpuCert == "")
                {
                    SelectCertificateButton.BorderBrush = Brushes.Red;
                    return;
                }

                if (inputPath == "")
                {
                    SelectUserPathButton.BorderBrush = Brushes.Red;
                    return;
                }

                Config config = new Config
                {

                    FTPServerPort = portFTP,
                    FTPUsername = usernameFTP,
                    FTPPassword = passwordFTP,
                    FilePath = inputPath,

                };

                ReadWriteConfig readWriteConfig = new ReadWriteConfig();
                ReadPFX readPFX = new ReadPFX();
                readPFX.CertificateReader2(inpuCert, passwordCert, config);
               
                ClientConfig clientConfig = new ClientConfig
                {
                    ServerAddress = serverAddress,
                    SFTPPort= int.Parse(portFTP),
                    Key = config.Key,
                    IV= config.IV,
                };

                readWriteConfig.WriteConfiguration(config);
                readWriteConfig.WriteConfigurationClient(clientConfig);
                MessageBox.Show("Zapisano konfigurację!", "OK!", MessageBoxButton.OK, MessageBoxImage.Information);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu konfiguracji", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string AddressValidation(string DBAddress)
        {
            var pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            if (!string.IsNullOrWhiteSpace(DBAddress))
            {
                if (!Regex.IsMatch(DBAddress, pattern) || Regex.IsMatch(DBAddress, "localhost"))
                {
                    MessageBox.Show("Obsługiwany wyłącznie IPv4 lub localhost", "Błąd danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";

                }
                else
                {
                    return DBAddress;

                }
            }
            else
            {
                MessageBox.Show("Adres abazy danych jest wymagany.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }
        private string SFTPPortValidation(string SFTPPort)
        {
            var pattern = @"^([0-9]|[1-9][0-9]{1,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$";

            if (!string.IsNullOrWhiteSpace(SFTPPort))
            {
                if (!Regex.IsMatch(SFTPPort, pattern))
                {
                    MessageBox.Show("Obsługiwany wyłącznie numery portów", "Błąd danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";

                }
                else
                {
                    return SFTPPort;

                }
            }
            else
            {
                MessageBox.Show("Port FTP jest wymagany.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }


        private void Start_Server_Button(object sender, EventArgs e)
        {
            try
            {

                Process.Start("..\\DataSerwer\\DataSerwer.exe");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Podczas uruchomienia serwera obsługującego klientów wystąpił błąd!", "Błąd startu serwera!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
        }

        private void Stop_Server_Button(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient("localhost", 3333);
                NetworkStream stream = client.GetStream();

                string plaintext = "STOP";
                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                byte[] data = Encoding.ASCII.GetBytes(encryptedMessage);
                stream.Write(data, 0, data.Length);

            }
            catch(Exception ex)
            {
                MessageBox.Show("Podczas zatrzymania serwera obsługującego klientów wystąpił błąd!", "Błąd zatrzymania serwera!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }
    }
}
