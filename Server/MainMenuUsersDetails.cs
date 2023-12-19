using DataServerGUI.AppWindows;
using System;
using System.Windows;

namespace Server
{
    public partial class MainMenu : Window
    {
        private void Add_User_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddUserWindow window = new AddUserWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania okna  dodawania użytkownika!");
            }
        }

        private void Del_User_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteUserWindow window = new DeleteUserWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania okna  usuwania użytkownika!");
            }
        }

        private void Edit_User_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditUserWindow window = new EditUserWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania okna  edycji użytkownika!");
            }
        }

    }
}
