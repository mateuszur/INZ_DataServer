using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

        private string username;// = "mateuszur";
        private string password;// = "Pa$$w0rd";
        private int user_id;
        private int user_privilege;


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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            username = UsernameTextBox.Text;
            password = PasswordBox.Visibility== Visibility.Visible ? PasswordBox.Password  : PasswordTextBox.Text;
        



            //using (SHA1 sha1 = SHA1.Create())
            //{
            //    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            //    password = BitConverter.ToString(hash).Replace("-", "").ToLower();
            //}

            

            try
            {
                TcpClient client = new TcpClient("192.168.1.90", 3333);
                NetworkStream stream = client.GetStream();

                //Obsługa rządania
                byte[] dataUser = Encoding.ASCII.GetBytes("Login " + username + " "+password);
                stream.Write(dataUser, 0, dataUser.Length);
                await Task.Delay(1500);

                //Obsługa odp string respon = sessionDetails.SessionID + " " + sessionDetails.DataTimeEnd + " "+userDetails.Privileges + " " + userDetails.Login + " " + userDetails.ID;
                dataUser = new byte[256];

                int bytes = stream.Read(dataUser, 0, dataUser.Length);
                string responseData = Encoding.ASCII.GetString(dataUser, 0, bytes);
                string[] parts = responseData.Split(',');


                if (parts[0]== "LoginSuccessful" && parts[3]=="1")
                    {
                        MessageBox.Show("Logowanie zakończone pomyślnie. Witaj: " + parts[4] +
                                   "\nTwoje uprawnienia to: " + parts[3] +   "\n ID sesji: "+ parts[1]);

                    user_privilege = int.Parse(parts[3]);    

                        MainMenu mainmenu = new MainMenu(user_privilege,username);
                        mainmenu.Show();
                        client.Close();
                        this.Close();

                    }
                    else
                    {
                        MessageBox.Show("Niepoprawna nazwa użytkownika lub hasła!");
                        connection_name.Close();
                    }
                
            }
            catch (Exception ex)
            {
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





        // funkcja do testowania aby pominąć wpisywanie logwagoania- do usunięcia na koniec
        private void LoginButton_Click1(object sender, RoutedEventArgs e)
        {
            MainMenu mainmenu = new MainMenu( user_privilege, username);
            mainmenu.Show();
        }
        }
}
