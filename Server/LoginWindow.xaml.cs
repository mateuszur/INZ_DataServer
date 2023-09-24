using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {


        private string username = "user";
        private string password = "Pa$$w0rd";
        private int user_id = 0;
        private int user_privilege = 0;


        //Database
        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();


        //Window
       


        public LoginWindow()
        { 

            connection_string = fileManager.ReadParameter();
            InitializeComponent();




        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //1username = UsernameTextBox.Text;
            //password = PasswordBox.Visibility== Visibility.Visible ? PasswordBox.Password  : PasswordTextBox.Text;
          //  NazwaUżytkownika.Content = username;

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            connection_name.ConnectionString = connection_string;

            try
            {
                connection_name.Open();
                {
                    string query1 = "SELECT * FROM `Users` WHERE Login LIKE @username AND Password LIKE @password;";
                    MySqlCommand command = new MySqlCommand(query1, connection_name);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    MySqlDataReader data_from_query1 = command.ExecuteReader();

                    while (data_from_query1.Read())
                    {
                        user_id = (int)data_from_query1["ID"];
                        user_privilege = (int)data_from_query1["Privileges"];
                    }

                    if (user_id != 0)
                    {
                        //MessageBox.Show("Logowanie zakończone pomyślnie. Witaj: " + username +
                        //           " Twoje uprawnienia to: " + user_privilege);
                        //EnableButtons();
                        //connection_name.Close();

                        //Content_Harmonogram.Visibility = Visibility.Visible;
                        //Content_Logowanie.Visibility = Visibility.Collapsed;

                        MainMenu mainmenu = new MainMenu(user_id,user_privilege,username);
                        mainmenu.Show();
                        connection_name.Close();
                        this.Close();

                    }
                    else
                    {
                        MessageBox.Show("Niepoprawna nazwa użytkownika lub hasła!");
                        connection_name.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia do bazy danych!" + ex);
                connection_name.Close();

            }
        
    }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Show the password
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide the password
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
