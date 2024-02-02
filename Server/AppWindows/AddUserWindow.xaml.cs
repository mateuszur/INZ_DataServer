using DataServerGUI.Configurations;
using MySql.Data.MySqlClient;
using Server;
using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataServerGUI.AppWindows
{
    /// <summary>
    /// Logika interakcji dla klasy AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        private string connection_string;

        ParametrFileManager fileManager = new ParametrFileManager();
        MySqlConnection connection_name = new MySqlConnection();

        string name;
        string surname;
        string login;
        string password;
        string password2;
        int privilege;
        string salt;
        string space_for_file;
      

        public AddUserWindow()
        {
            InitializeComponent();
            connection_string = fileManager.ReadParameter();
            connection_name.ConnectionString = connection_string;
        }

        public void Add_User_Save(object sender, RoutedEventArgs e)
        {
            try
            {

                name= NameTextBox.Text;
                surname= SurnameTextBox.Text;
                login= LoginTextBox.Text;
                password=Password1.Password;
                password2= Password2.Password;
                space_for_file = Space_available.Text;

            
                    if (Name_Surname_Validation(name) == "")
                    {
                        NameTextBox.BorderBrush = System.Windows.Media.Brushes.Red; NameTextBox.Focus();
                        return;
                    }
                  

                    if (Name_Surname_Validation(surname) == "")
                    {
                        SurnameTextBox.BorderBrush = System.Windows.Media.Brushes.Red; SurnameTextBox.Focus();
                    return;
                }
                  
                    if (Login_Validation(login) == "")
                    {
                        LoginTextBox.BorderBrush = System.Windows.Media.Brushes.Red; LoginTextBox.Focus();
                    return;
                }
                   

                    if (Password_Validation(password, password2) == "")
                    {
                        Password1.Background =  System.Windows.Media.Brushes.Red; Password1.Focus();
                        Password2.Background = System.Windows.Media.Brushes.Red;
                    return;
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
                        Space_available.BorderBrush = System.Windows.Media.Brushes.Red; Space_available.Focus();
                    return;
                }
                    

                //Solenie hasła:
                salt = Salt_Generator();
                Add_User();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Podczas przetwarzania formularza wystąpił błąd!", "Błąd danych!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            

        }


        private string  Name_Surname_Validation(string name_surname)
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


        private string Login_Validation(string login)
        {
            if (!string.IsNullOrWhiteSpace(login))
            {
                if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
                {
                    MessageBox.Show("Login może zawierać tylko litery i cyfry.", "Błąd danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";

                }
                else
                {
                    return login;

                }
            }
            else
            {
                MessageBox.Show("Login jest wymagany.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        private string Password_Validation(string password, string password2)
        {

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";

            if (!string.IsNullOrWhiteSpace(password) || !string.IsNullOrEmpty(password2))
            {
                if(Regex.IsMatch(password, pattern))
                {
                    if(password==password2)
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
                    MessageBox.Show("Hasło musi zawierać:\n -co najmniej 8 znaków,\n -w tym co najmniej jedną cyfrę,\n -jeden znak specjalny,\n -jedną dużą literę i jedną małą literę.\"","Błąd hasła.",  MessageBoxButton.OK, MessageBoxImage.Error);
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

        private string Salt_Generator()
        {
            //Długość soli w bajtach
            int saltLength = 32; 

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        
            byte[] saltBytes = new byte[saltLength];

            //Generowanie soli
            rng.GetNonZeroBytes(saltBytes);

            //Przekształcenie tablicy bajtów na ciąg znaków w formacie Base64
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

        private void Add_User()
        {
            try
            {
                connection_name.Open();

                string sql =
              "INSERT INTO `Users` (`Login`, `Password`, `Salt`, `Privileges`, `Name`, `Surname`, `Space_available`,`Active`) VALUES (@login, @password, @salt, @privileges, @name, @surname, @space_available, '1')";



                MySqlCommand command = new MySqlCommand(sql, connection_name);
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", PasswordSalting(password, salt));
                command.Parameters.AddWithValue("@salt", salt);
                command.Parameters.AddWithValue("@privileges", privilege);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@surname", surname);
                command.Parameters.AddWithValue("@space_available", space_for_file);



                command.ExecuteNonQuery();
                connection_name.Close();


                MessageBox.Show("Z powodzeniem dodano użytkownika " + login + " !");


            }
            catch (Exception ex)
            {
                MessageBox.Show("Podczas dodawania rekordu do bazy danych wystąpił błąd! \n " + ex.ToString(), "Błąd bazy!", MessageBoxButton.OK, MessageBoxImage.Error);
                connection_name.Close();
                return;
            }
        }

    }



   

}
