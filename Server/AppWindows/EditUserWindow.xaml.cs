using DataServerGUI.Configurations;
using MySql.Data.MySqlClient;
using Server;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DataServerGUI.AppWindows
{
    /// <summary>
    /// Logika interakcji dla klasy EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        private string connection_string;

        ParametrFileManager fileManager = new ParametrFileManager();
        MySqlConnection connection_name = new MySqlConnection();

        string name;
        string surname;
        string password="";
        string password2="";
        int privilege;
        string salt;
        string space_for_file;
        bool radioButtonChecked = false;


        public EditUserWindow()
        {
            InitializeComponent();
            connection_string = fileManager.ReadParameter();
            connection_name.ConnectionString = connection_string;
        }

        private void IDTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public void IDTextBox_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (IDTextBox.Text == "")
                    {
                        NameTextBox.Text = "";
                        SurnameTextBox.Text = "";
                        Password1.Password = "";
                        Password2.Password = "";
                        Space_available.Text = "";
                        return;
                    }

                    connection_name.Open();
                    string sql0 = "SELECT COUNT(ID) FROM `Users` WHERE  Name NOT LIKE \"\" AND ID=" + IDTextBox.Text + ";";
                    string sql1 = "SELECT Privileges, Name, Surname, Space_available FROM `Users` where `ID` = " + IDTextBox.Text + ";";

                    MySqlCommand command0 = new MySqlCommand(sql0, connection_name);
                    MySqlDataReader data_from_querry0 = command0.ExecuteReader();


                    data_from_querry0.Read();
                    int number = data_from_querry0.GetInt32(0);

                    data_from_querry0.Close();


                    if (number == 0)
                    {
                        MessageBox.Show("Najwyraźniej brak użytkownika o podanym ID...  ");
                        connection_name.Close();

                        NameTextBox.Text = "";
                        SurnameTextBox.Text = "";
                        Password1.Password = "";
                        Password2.Password = "";
                        Space_available.Text = "";


                    }
                    else
                    {
                        MySqlCommand command1 = new MySqlCommand(sql1, connection_name);
                        MySqlDataReader data_from_querry1 = command1.ExecuteReader();


                        while (data_from_querry1.Read())
                        {
                            privilege = int.Parse(data_from_querry1[0].ToString());
                            NameTextBox.Text = (data_from_querry1[1]).ToString();
                            SurnameTextBox.Text = (data_from_querry1[2]).ToString();
                            Space_available.Text= (data_from_querry1[3].ToString());

                        }
                        connection_name.Close();
                        NameTextBox.IsReadOnly = false;
                        SurnameTextBox.IsReadOnly=false;
                        radioButtonPasswor.IsEnabled = true;
                        PrivilegesComboBox.IsEnabled = true;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Najwyraźniej brak użytkownika o podanym ID...");
                    return;
                }
                connection_name.Close();
            }
        }

        public void Edit_User_Save(object sender, EventArgs e)
        {
            try
            {

                name = NameTextBox.Text;
                surname = SurnameTextBox.Text;
              
                password = Password1.Password;
                password2 = Password2.Password;
                space_for_file = Space_available.Text;


                if (Name_Surname_Validation(name) == "")
                {
                    NameTextBox.BorderBrush = Brushes.Red; NameTextBox.Focus();
                    return;
                }


                if (Name_Surname_Validation(surname) == "")
                {
                    SurnameTextBox.BorderBrush = Brushes.Red; SurnameTextBox.Focus();
                    return;
                }




                if (radioButtonChecked)
                {
                    if (Password_Validation(password, password2) == "")
                    {
                        Password1.Background = Brushes.Red; Password1.Focus();
                        Password2.Background = Brushes.Red;
                        return;
                    }
                }
                


                if (PrivilegesComboBox.SelectedItem != null)
                {
                    ComboBoxItem selectedOption = (ComboBoxItem)PrivilegesComboBox.SelectedItem;
                    string selectedValue = selectedOption.Content.ToString();


                    switch (selectedValue)
                    {
                        case "Użytkownik sytemu":
                            privilege = 2;
                            break;

                        case "Administrator systemu":

                            privilege = 1;
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Błąd walidacji", "Podaj poziomu upranień użytkownika!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Space_For_File_Validation(space_for_file) == "")
                {
                    Space_available.BorderBrush = Brushes.Red; Space_available.Focus();
                    return;
                }


                //Solenie hasła:
                salt = Salt_Generator();
                Update_User();
                NameTextBox.Text = "";
                SurnameTextBox.Text = "";
                Password1.Password = "";
                Password2.Password = "";
                Space_available.Text = "";
                NameTextBox.IsReadOnly = true;
                SurnameTextBox.IsReadOnly = true;
                radioButtonPasswor.IsEnabled = false;
                PrivilegesComboBox.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Podczas przetwarzania formularza wystąpił błąd!", "Błąd danych!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
           
        }


        private void Update_User()
        {
            try
            {
                connection_name.Open();
               
                string sql =
                "UPDATE `Users` SET `Password` = @password, `Salt` = @salt, `Privileges` = @privileges, `Name` = @name, `Surname` = @surname, `Space_available` = @space_available WHERE `Users`.`ID` = @userID;";
                string sql2= "UPDATE `Users` SET `Privileges` = @privileges, `Name` = @name, `Surname` = @surname, `Space_available` = @space_available WHERE `Users`.`ID` = @userID;";
                MySqlCommand command; 
                if (radioButtonChecked)
                {
                    command = new MySqlCommand(sql, connection_name);

                    command.Parameters.AddWithValue("@password", PasswordSalting(password, salt));
                    command.Parameters.AddWithValue("@salt", salt);
                 

                }
                else
                {
                    command= new MySqlCommand(sql2,connection_name);

                }
                command.Parameters.AddWithValue("@privileges", privilege);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@surname", surname);
                command.Parameters.AddWithValue("@space_available", space_for_file);
                command.Parameters.AddWithValue("@userID", IDTextBox.Text);

                command.ExecuteNonQuery();
                connection_name.Close();


                MessageBox.Show("Z powodzeniem edytowano użytkownika o  ID: " + IDTextBox.Text + "!");


            }
            catch (Exception ex)
            {
                connection_name.Close();
                MessageBox.Show("Podczas edytowania rekordu w bazie danych wystąpił błąd!", "Błąd bazy!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private string Salt_Generator()
        {
            //długość soli w bajtach
            int saltLength = 32;
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();


            byte[] saltBytes = new byte[saltLength];

            //generowanie soli
            rng.GetNonZeroBytes(saltBytes);

            // Przekształć tablicę bajtów na ciąg znaków w formacie Base64
            string salt = Convert.ToBase64String(saltBytes);

            return salt;
        }

        private string PasswordSalting(string password, string salt)
        {
            password = password + salt;
            string salted_password;
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                salted_password = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            return salted_password;

        }
        private string Name_Surname_Validation(string name_surname)
        {
            if (!string.IsNullOrWhiteSpace(name_surname))
            {
                if (!Regex.IsMatch(name_surname, @"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ]+$"))
                {
                    MessageBox.Show("Imię i nazwisko mogą zawierać tylko litery.", "Błąd danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";

                }
                else
                {
                    // Przekształć imię tak, aby pierwsza litera była wielka, a pozostałe małe
                    string formattedNameSurname = char.ToUpper(name_surname[0]) + name_surname.Substring(1).ToLower();
                    return formattedNameSurname;

                }
            }
            else
            {
                MessageBox.Show("Imię i Nazwisko są wymagane.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }

        }



        private string Password_Validation(string password, string password2)
        {

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";

            if (!string.IsNullOrWhiteSpace(password) || !string.IsNullOrEmpty(password2))
            {
                if (Regex.IsMatch(password, pattern))
                {
                    if (password == password2)
                    {
                        return password;
                    }
                    else
                    {
                        MessageBox.Show("Niezgodność haseł!.", "Hasła nie są jednakowe", MessageBoxButton.OK, MessageBoxImage.Error);
                        return "";
                    }
                }
                else
                {
                    MessageBox.Show("Hasło musi zawierać:\n -co najmniej 8 znaków,\n -w tym co najmniej jedną cyfrę,\n -jeden znak specjalny,\n -jedną dużą literę i jedną małą literę.\"", "Błąd hasła.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
            else
            {
                MessageBox.Show("Hasło jest wymagane.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        private string Space_For_File_Validation(string space_for_file)
        {
            if (!string.IsNullOrWhiteSpace(space_for_file))
            {
                if (!Regex.IsMatch(space_for_file, @"^\d+(\.0|\.5)?$"))
                {
                    MessageBox.Show("Możesz podać dostępne miejsce dla użytkownika w GB z dokładnoscią do jednego miejsca po przecinku (0.0 lub 0.5).", "Błąd danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";

                }
                else
                {
                    return space_for_file;

                }
            }
            else
            {
                MessageBox.Show("Dostępne miejsce jest wymagane.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }


        public void Password_Change_RadioButton(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if(radioButton.IsChecked == true)
            {
                radioButtonChecked = true;
                Password1.IsEnabled = true;
                Password2.IsEnabled = true;
            }
        }
    }
}
