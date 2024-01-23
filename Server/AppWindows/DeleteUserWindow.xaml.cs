using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Server;
using System;
using System.Windows;

namespace DataServerGUI.AppWindows
{
    /// <summary>
    /// Logika interakcji dla klasy DeleteUserWindow.xaml
    /// </summary>
    /// 



    public partial class DeleteUserWindow : Window
    {
        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;

        MySqlConnection connection_name = new MySqlConnection();

        public DeleteUserWindow()
        {
            InitializeComponent();
            connection_string = fileManager.ReadParameter();
            connection_name.ConnectionString = connection_string;
        }

        public void Delete_User_Save(object sender, EventArgs e)
        {
            try
            {
                if (IdTextBox.Text == "")
                {
                    return;
                }

                connection_name.Open();
                string sql0 = "SELECT COUNT(ID) FROM `Users` WHERE  Login NOT LIKE \"\" AND ID=" + IdTextBox.Text + ";";
                string sql1 =
                "UPDATE `Users` SET `Login` = '', `Password` = '', `Salt` = '', `Privileges` = '0', `Name` = '', `Surname` = '', `Space_available` = '0', `Active` = '0' WHERE `Users`.`ID` = @userID";


                MySqlCommand command0 = new MySqlCommand(sql0, connection_name);
            
                MySqlDataReader data_from_querry0 = command0.ExecuteReader();

                data_from_querry0.Read();
                int number = data_from_querry0.GetInt32(0);
                data_from_querry0.Close();

                if (number == 0)
                {
                    MessageBox.Show("Najwyraźniej brak użytkownika o podanym ID...  ");
                    connection_name.Close();
                }
                else
                {

                    MySqlCommand command1 = new MySqlCommand(sql1, connection_name);
                    command1.Parameters.AddWithValue("@userID", IdTextBox.Text);
                    command1.ExecuteNonQuery();
                    connection_name.Close();


                    MessageBox.Show("Z powodzeniem usunięto użytkownika o ID: " + IdTextBox.Text + "!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Podczas usuwaniu rekordu w bazie danych wystąpił błąd!", "Błąd bazy!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }

    
}
