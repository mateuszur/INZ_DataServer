using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {

        private int user_privilege;

        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();


        public MainMenu(int user_id, int user_privilege, string username)
        {
            InitializeComponent();
            connection_string = fileManager.ReadParameter();
            this.user_privilege = user_privilege;

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
                if (user_privilege == 1 || user_privilege == 2)
                {
                    connection_name.Open();

                    string querry = "SELECT ID, Login, Name, Surname, Space_available, Disk_space_used FROM `Users`;";

                    MySqlCommand commend = new MySqlCommand(querry, connection_name);
                    MySqlDataReader data_from_querry = commend.ExecuteReader();

                    var listOfUsers = new List<Entry_Users>();


                    while (data_from_querry.Read())
                    {
                      
                        Entry_Users feld = new Entry_Users
                        {
                            ID = int.Parse(data_from_querry.GetString(0)),
                            Login = data_from_querry.GetString(1),
                            Name = data_from_querry.GetString(2),
                            Surname= data_from_querry.GetString(3),
                            Space_available = data_from_querry.GetString(4),
                            Disk_space_used = data_from_querry.GetString(5),
                        
                        };


                        listOfUsers.Add(feld);
                    }

                    MessageBox.Show("User_name: " + listOfUsers[0].Name + "\n");

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
                MessageBox.Show("Otwieranie zarządzania hermonogramem... \n"+ ex);
            }


        }

        private void ChangePageVisibility(Grid pageToShow)
        {
            Server_Setting_Content.Visibility = Visibility.Collapsed;
            Server_Users_Content.Visibility = Visibility.Collapsed;

            pageToShow.Visibility = Visibility.Visible;
        }
    }
}
