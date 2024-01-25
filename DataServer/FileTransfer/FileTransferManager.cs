using DataServerService.Configurations;
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
        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();

        //weryfikacja sesji przez przystąpieniem do przeysłania pliku
        public bool IsSessionValid(string sessionID,int userID)
        {
            if(IsSessionValidWorker(sessionID, userID))
            {
                return true;
            }
            return false;
        }
        private bool IsSessionValidWorker(string sessionID,int userID)
        {
            try
            {
                connection_name.ConnectionString = connection_string;

                string query = "SELECT COUNT(ID) FROM `View_Session` WHERE ID like @sessionID AND User_ID= @userID";
                int result=0;
                MySqlCommand command = new MySqlCommand(query, connection_name);
                connection_name.Open();
                command.Parameters.AddWithValue("@sessionID", sessionID);
                command.Parameters.AddWithValue("@userID", userID);
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

                string querry = "INSERT INTO `File_Table` (`ID`, `User_ID`, `File_name`, `File_Size`, `File_type` , `Date_of_Transfer`, `Source_IP_Adress`) VALUES(@fileID, @userID, @fileName, @fileSize, @fileType, @dateTransfer, @sourceIP)";
                MySqlCommand command = new MySqlCommand(querry, connection_name);

                command.Parameters.AddWithValue("@fileID", fileDetails.FileID);
                command.Parameters.AddWithValue("@userID", fileDetails.userID);
                command.Parameters.AddWithValue("@fileName", fileDetails.FileName);
                command.Parameters.AddWithValue("@fileSize", fileDetails.FileSize);
                command.Parameters.AddWithValue("@fileType", fileDetails.FileType);
                command.Parameters.AddWithValue("@dateTransfer", fileDetails.DateOfTransfer);
                command.Parameters.AddWithValue("@sourceIP", fileDetails.SourceIPAddress);


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
                    
                }
                else
                {
                    return;
                }
            }catch (Exception ex)
            {
                Console.WriteLine("  Błąd podczas tworzenia folderu użytkownika: "+ex.Message);
                return;
            }
        }


        //user/User+ID/fileName
        public string CreateFileRespon(FileDetails fileDetails)
        {
            try
            {
                string responString = "/user/User" + fileDetails.userID + "/" + fileDetails.FileID  + fileDetails.FileType;
                return responString;

            }
            catch (Exception ex)
            {
                string error = "error";
                Console.WriteLine(ex.Message);
                return error;            }

        }


        public List<FileDetails> GetFileList(List<FileDetails>listOfFiles, int userID)
        {
            try
            {
                connection_name.ConnectionString = connection_string;



                string querry = "SELECT `File_name`, File_Size, File_type, Date_of_Transfer FROM `View_Files_List` WHERE User_ID= @userID;";


                MySqlCommand command = new MySqlCommand(querry, connection_name);

                command.Parameters.AddWithValue("@userID", userID);

                connection_name.Open();
                MySqlDataReader data_from_querry = command.ExecuteReader();

                while (data_from_querry.Read())
                {
                    FileDetails feld = new FileDetails
                    {
                        FileName = data_from_querry.GetString(0),
                        FileSize = int.Parse(data_from_querry.GetString(1)),
                        FileType = data_from_querry.GetString(2),
                        DateOfTransfer = DateTime.ParseExact(data_from_querry.GetString(3), "dd.MM.yyyy HH:mm:ss", null),
                    };
                    listOfFiles.Add(feld);

                }
                connection_name.Close();


                return listOfFiles;
            }catch (Exception ex)
            {
                Console.WriteLine("  Błąd pobiernaia listy plików:"+ex.Message);
                connection_name.Close();
                listOfFiles = null;
                return listOfFiles;
            }
        }


        public string GetFileListRespon(List<FileDetails> listOfFiles)
        {
            string respon="";


            foreach (FileDetails file in listOfFiles)
            {

                respon = respon+file.FileName+" "+file.FileSize+" "+file.FileType+" "+file.DateOfTransfer+",";

            }


            return respon;
        }

    }
}
