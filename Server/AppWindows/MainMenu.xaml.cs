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

namespace Server
{

    public partial class MainMenu : Window
    {

        private int user_privilege;
        private string username;
        private string dbAddress = "";
        private string usernameFTP="";
        private string passwordFTP="";
        private string portFTP = "";
        private string inpuCert = "";
        private string passwordCert;
        private string inputPath = "";

        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();


        public MainMenu(int user_privilege, string username)
        {
            InitializeComponent();
            connection_string = fileManager.ReadParameter();
            this.user_privilege = user_privilege;
            this.username = username;

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
                            ID = int.Parse(data_from_querry.GetString(0)),
                            Login = data_from_querry.GetString(1),
                            Privileges = data_from_querry.GetString(2),
                            Name = data_from_querry.GetString(3),
                            Surname = data_from_querry.GetString(4),
                            Space_available = data_from_querry.GetString(5) ?? "0",
                            Disk_space_used = data_from_querry.GetString(6) ?? "0",

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
                connection_name.Close();
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
            }catch (Exception ex)
            {
                MessageBox.Show("Probem z dostępem do plików!.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SelectUserPath(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName="Pliki DataSerwer",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    ValidateNames = false
                };

                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    inputPath = dialog.FileName;

                    UserPath.TextWrapping = TextWrapping.WrapWithOverflow;
                    UserPath.Text = inputPath;
                }


            }
            catch(Exception ex) 
            {
                MessageBox.Show("Probem z dostępem do plików!.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Save_Server_Config(object sender, RoutedEventArgs e)
        {
            try
            {
                dbAddress = DBAddress.Text;
                usernameFTP = FTPUsername.Text;
                passwordFTP = FTPPassword.Password;
                portFTP = SFTPPort.Text;
                passwordCert = CertificatePass.Password;

                if (DataBaseAddressValidation(dbAddress) == "")
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

                readPFX.CertificateReader(inpuCert, passwordCert, config);
               // readWriteConfig.WriteConfiguration(config);
            }catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu konfiguracji", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string DataBaseAddressValidation(string DBAddress)
        {
            var pattern= @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

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
                if (!Regex.IsMatch(SFTPPort, pattern) )
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



    }
}
