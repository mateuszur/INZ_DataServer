using DataServerGUI.Configurations;
using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        private string username;
        private string password;


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


        private  void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            username =UsernameTextBox.Text;
            password = PasswordBox.Visibility== Visibility.Visible ? PasswordBox.Password  : PasswordTextBox.Text;


            connection_name.ConnectionString = connection_string;


            UserDetails userDetails = new UserDetails();
            string user_password = "";
            string password_salt = "";
            string password_temp;


            try
            {
                connection_name.Open();

                string query = "SELECT * FROM `View_Users_Login` WHERE Login LIKE @username;";

                MySqlCommand command = new MySqlCommand(query, connection_name);
                command.Parameters.AddWithValue("@username", username);

                MySqlDataReader data_from_query = command.ExecuteReader();

                while (data_from_query.Read())
                {
                    userDetails.ID = (int)data_from_query["ID"];
                    userDetails.Login = data_from_query["Login"].ToString();
                    user_password = data_from_query["Password"].ToString();
                    password_salt = data_from_query["Salt"].ToString();
                    userDetails.Privileges = (int)data_from_query["Privileges"];
                }

                connection_name.Close();
            


                //Weryfiakcja hasła i username
                if (password_salt == "")
                {
                    password_temp = password;
                }
                else
                {
                    password_temp = password + password_salt;
                }

                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password_temp));
                    password_temp = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                if (userDetails.Login == username && user_password == password_temp)
                {



                    MainMenu mainmenu = new MainMenu(userDetails.Privileges, userDetails.Login);
                    mainmenu.Show();
                    this.Close();
                }
                else {
                    MessageBox.Show("Niepoprawna nazwa użytkownika lub hasła!");
                }


                //Weryfiakcja hasła i username
            }

            catch (Exception ex)
            {
                connection_name.Close();
                MessageBox.Show("Błąd połączenia do serwera!\n" + ex.ToString());


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
