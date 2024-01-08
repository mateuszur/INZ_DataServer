using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataServerService
{
    public class FileTransferManager : FileDetails
    {
        //dodać zaczytranie z pliku
        string filePath = "C:\\Users\\Administrator\\Desktop\\TestUser";

        //obiekt dla potrze przetwarzania zapytania o utworzenie/ usunięcie/ wyświetlenie listy plikó
        private FileDetails fileDetails = new FileDetails();

        //Baza danych do dodania zaczytanie z pliku konfiguracyjnego
        private string connection_string = "Server=192.168.1.51;Uid=inz;Pwd=Pa$$w0rd;Database=Server_inz_MU23/24;";
        private MySqlConnection connection_name = new MySqlConnection();

        //weryfikacja sesji przez przystąpieniem do przeysłania pliku
        public bool IsSessionValid(string sessionID)
        {
            if(IsSessionValidWorker(sessionID))
            {
                return true;
            }
            return false;
        }
        private bool IsSessionValidWorker(string sessionID)
        {
            try
            {
                connection_name.ConnectionString = connection_string;

                string query = "SELECT COUNT(ID) FROM `View_Session` WHERE ID like @sessionID";
                int result=0;
                MySqlCommand command = new MySqlCommand(query, connection_name);
                connection_name.Open();
                command.Parameters.AddWithValue("@sessionID", sessionID);
                MySqlDataReader data_from_querry = command.ExecuteReader();
                while (data_from_querry.Read())
                {
                    result = int.Parse(data_from_querry.GetString(0));
                }

                if (result == 1 )
                {
                    connection_name.Close();
                    return true;
                }
                else
                {
                    connection_name.Close();
                    return false;
                }
            }catch (Exception ex)
            {
                connection_name.Close();
                return false;
            }
        }
        //weryfikacja sesji przez przystąpieniem do przeysłania pliku




        //weryfiakcja dsotępności miejca w systemie dla użytkownika
        public bool HasUserFreeSpace(FileDetails fileDetails)
        {
            if(HasUserFreeSpaceWorke(fileDetails)) {
           
                return true;

            }else { return false; }

        }
        
        private bool HasUserFreeSpaceWorke(FileDetails fileDetails)
        {
            try
            {
                connection_name.ConnectionString = connection_string;
                
                string querry = "SELECT Disk_space_used, Space_available FROM `View_Disk_space_used_by_Users` WHERE ID= @userID;";
                int Disk_space_used = 0;
                int Space_available = 0;

                MySqlCommand command = new MySqlCommand(querry, connection_name);

                command.Parameters.AddWithValue("@userID", fileDetails.userID);

                connection_name.Open();
                MySqlDataReader data_from_querry = command.ExecuteReader();

                while(data_from_querry.Read())
                {
                    Disk_space_used= int.Parse(data_from_querry.GetString(0));
                    Space_available = int.Parse(data_from_querry.GetString(1));
                }
                connection_name.Close();

                if((Disk_space_used+ fileDetails.FileSize)<Space_available)
                {
                    return true;

                }else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                return false;
            }

        }
        //weryfiakcja dsotępności miejca w systemie dla użytkownika




        // obsługa tworzenie pliku oraz przygotowanie odp dla klienta
        public void CreateFile(FileDetails fileDetails)
        {
            CreateFileWorker(fileDetails);
        }

        private void CreateFileWorker(FileDetails fileDetails)
        {
            try
            {
                connection_name.ConnectionString = connection_string;
                CreateFileID(fileDetails);

                string querry = "INSERT INTO `File_Table` (`ID`, `User_ID`, `File_name`, `File_Size`, `File_type`) VALUES(@fileID, @userID, @fileName, @fileSize, @fileType)";
                MySqlCommand command = new MySqlCommand(querry, connection_name);

                command.Parameters.AddWithValue("@fileID", fileDetails.FileID);
                command.Parameters.AddWithValue("@userID", fileDetails.userID);
                command.Parameters.AddWithValue("@fileName", fileDetails.FileName);
                command.Parameters.AddWithValue("@fileSize", fileDetails.FileSize);
                command.Parameters.AddWithValue("@fileType", fileDetails.FileType);


                connection_name.Open();
                command.ExecuteNonQuery();
                connection_name.Close();

                CheckCreatePath(filePath, fileDetails);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void  CreateFileID(FileDetails fileDetails)
        {
            try
            {
                string tempFileID = fileDetails.FileName + fileDetails.DateOfTransfer.ToString();

                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(tempFileID));
                    tempFileID = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                fileDetails.FileID = tempFileID;
            }catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                    }
        }


        private void CheckCreatePath(string path, FileDetails fileDetails)
        {
            try
            {
                path = path + "\\User" + fileDetails.userID;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return;
                    //Console.WriteLine("Folder został utworzony: {0}", Directory.GetCreationTime(path));
                }
                else
                {
                    return;
                }
            }catch (Exception ex)
            {
                Console.WriteLine("  Błąd podczas tworzenia folderu usera: "+ex.Message);
                return;
            }
        }


        //user/User+ID/fileName
        public string CreateFileRespon(FileDetails fileDetails)
        {
            try
            {
                string responString = "/user/User" + fileDetails.userID + "/" + fileDetails.FileID + "." + fileDetails.FileType;
                return responString;

            }
            catch (Exception ex)
            {
                string error = "error";
                Console.WriteLine(ex.Message);
                return error;            }

        }







    }
}
