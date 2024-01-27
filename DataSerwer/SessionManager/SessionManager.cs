using DataSerwer.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataSerwer.SessionManager
{
    public class SessionManager
    {


        //Baza danych
        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();

        private UserDetails userDetails = new UserDetails();

        private SessionDetails sessionDetails = new SessionDetails();

        public SessionManager()
        {
            connection_string = fileManager.ReadParameter();
            connection_name.ConnectionString = connection_string;
        }

        //przygotowuje odpowiedź dla klieta po utworzeniu sesji
        public string SessionRespon()
        {

            string respon = sessionDetails.SessionID + "," + sessionDetails.DataTimeEnd + "," + userDetails.Privileges + "," + userDetails.Login + "," + userDetails.ID;
            return respon;
        }


        //obsługa procesu tworzenia sesej w DB
        public bool IsSessionCreated(DateTime dateTime)
        {
            sessionDetails.DataTimeStart = dateTime;

            if (IsSessionCreatedWorker())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsSessionCreatedWorker()
        {
            string tempSesionID;
            connection_name.ConnectionString = connection_string;

            try
            {
                {
                    //zapytanie do bazy
                    string query = "INSERT INTO `Sesion` (`ID`, `User_ID`, `Start_sesion_Date`, `End_Sesion_Date`, `Source_Divice`, `Source_IP_Adress`, `Active`) VALUES (@SessionID, @userID, @startDate, @endDate, 'nd', '0.0.0.0', '1')";



                    sessionDetails.DataTimeEnd = sessionDetails.DataTimeStart.AddHours(24);

                    tempSesionID = userDetails.Login + sessionDetails.DataTimeStart.ToString();

                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(tempSesionID));
                        tempSesionID = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    }

                    sessionDetails.SessionID = tempSesionID;



                    MySqlCommand command = new MySqlCommand(query, connection_name);
                    command.Parameters.AddWithValue("@SessionID", sessionDetails.SessionID);
                    command.Parameters.AddWithValue("@userID", userDetails.ID);
                    command.Parameters.AddWithValue("@startDate", sessionDetails.DataTimeStart);
                    command.Parameters.AddWithValue("@endDate", sessionDetails.DataTimeEnd);


                    //Połączenie do bazy
                    connection_name.Open();

                    command.ExecuteNonQuery();

                    connection_name.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n");
                connection_name.Close();
                return false;
            }

        }
        // // /// /////////////////////////////////

        //obsługa logowania i weryfiakcji danych w bazie 
        public bool IsValidUser(string username, string password)
        {

            // sprawdź, czy nazwa użytkownika i hasło są poprawne.

            if (IsValidUserWorker(username, password))
            {
                return true;
            }
            return false;
        }


        private bool IsValidUserWorker(string username, string password)
        {
            connection_name.ConnectionString = connection_string;



            string user_password = "";
            string password_salt = "";

            string password_temp;

            try
            {
                //Połączenie do bazy
                connection_name.Open();
                {

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
                    //Połączenie do bazy


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

                        return true;
                    }
                    else { return false; }

                    //Weryfiakcja hasła i username

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;

            }




        }
        // // /// /////////////////////////////////

        // zakańczanie sesji- wylogorawnie 
        public bool EndSession(string SesionID)
        {
            try
            {

                string connection_string = "Server=192.168.1.51;Uid=inz;Pwd=Pa$$w0rd;Database=Server_inz_MU23/24;";
                MySqlConnection connection_name = new MySqlConnection();
                connection_name.ConnectionString = connection_string;
                connection_name.Open();

                string sqlQery = "SELECT COUNT(*) FROM `Sesion` where ID = @sesionID;";

                MySqlCommand command = new MySqlCommand(sqlQery, connection_name);
                command.Parameters.AddWithValue("@sesionID", SesionID);

                if (command.ExecuteNonQuery() == 1)
                {

                    string sqlQery2 = "UPDATE `Sesion` SET `Active` = 0 WHERE `ID`= @sesionID;";
                    MySqlCommand command2 = new MySqlCommand(sqlQery2, connection_name);
                    command.Parameters.AddWithValue("@sesionID", SesionID);

                }

                connection_name.Close();
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        ///werwyfiakcaja ważności seseji po zapytaniu klienta
        ///
        public bool IsSessionValid(string sessionID, string userID)
        {
            if (IsSessionValidWorker(sessionID, userID))
            {
                return true;
            }
            return false;
        }


        private bool IsSessionValidWorker(string sessionID, string userID)
        {
            try
            {
                MySqlConnection connection_name = new MySqlConnection();
                connection_name.ConnectionString = connection_string;

                string query = "SELECT COUNT(ID) FROM `View_Session` WHERE ID like @sessionID AND User_ID = @userID And Active= 1";
                string data = "";
                MySqlCommand command = new MySqlCommand(query, connection_name);

                command.Parameters.AddWithValue("@sessionID", sessionID);
                command.Parameters.AddWithValue("@userID", userID);

                connection_name.Open();
                MySqlDataReader data_from_query = command.ExecuteReader();



                while (data_from_query.Read())
                {
                    data = data_from_query[0].ToString();
                }


                if (data == "1")
                {
                    connection_name.Close();
                    return true;
                }
                else
                {
                    connection_name.Close();
                    return false;
                }

            }
            catch (Exception ex)
            {
                connection_name.Close();
                Console.WriteLine("Wystąpił błąd podczas weryfiakcji sesji: " + ex.ToString());
                return false;
            }


        }



    }
}
