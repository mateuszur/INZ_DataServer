using DataSerwer.Configuration;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mysqlx.Session;
using Mysqlx.Crud;

namespace DataSerwer.FileTransfer
{
    public class FileTransferManager : FileDetails
    {


        private int dataServerPort = 0;
        private string ftpServerPort ="";
        private string ftpUsername ="";
        private string ftpPassword = "";
        private string filePath = "";

        ParametrFileManager fileManager = new ParametrFileManager();
        private string connection_string;
        MySqlConnection connection_name = new MySqlConnection();

        ReadWriteConfig configReadWrite = new ReadWriteConfig();
        Config config = new Config();

        public FileTransferManager()
        {

            connection_string = fileManager.ReadParameter();
            connection_name.ConnectionString = connection_string;
            configReadWrite.ReadConfiguration(config);
            
            
            this.dataServerPort = config.DataServerPort;
            this.ftpServerPort = config.FTPServerPort;
            this.ftpUsername = config.FTPUsername;
            this.ftpPassword = config.FTPPassword;
            this.filePath= config.FilePath;
        }


        //weryfikacja sesji przez przystąpieniem do przeysłania pliku
        public bool IsSessionValid(string sessionID, int userID)
        {
            if (IsSessionValidWorker(sessionID, userID))
            {
                return true;
            }
            return false;
        }
        private bool IsSessionValidWorker(string sessionID, int userID)
        {
            try
            {

                string query = "SELECT COUNT(ID) FROM `View_Session` WHERE ID like @sessionID AND User_ID= @userID";
                int result = 0;
                MySqlCommand command = new MySqlCommand(query, connection_name);
                connection_name.Open();
                command.Parameters.AddWithValue("@sessionID", sessionID);
                command.Parameters.AddWithValue("@userID", userID);
                MySqlDataReader data_from_querry = command.ExecuteReader();
                while (data_from_querry.Read())
                {
                    result = data_from_querry.GetInt32(0);
                }

                if (result == 1)
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
                return false;
            }
        }
        //weryfikacja sesji przez przystąpieniem do przeysłania pliku




        //weryfiakcja dsotępności miejca w systemie dla użytkownika
        public bool HasUserFreeSpace(FileDetails fileDetails)
        {
            if (HasUserFreeSpaceWorke(fileDetails))
            {

                return true;

            }
            else { return false; }

        }

        private bool HasUserFreeSpaceWorke(FileDetails fileDetails)
        {
            try
            {


                string querry = "SELECT Disk_space_used, Space_available FROM `View_Disk_space_used_by_Users` WHERE ID= @userID;";
                double Disk_space_used = 0;
                double Space_available = 0;

                MySqlCommand command = new MySqlCommand(querry, connection_name);

                command.Parameters.AddWithValue("@userID", fileDetails.userID);

                connection_name.Open();
                MySqlDataReader data_from_querry = command.ExecuteReader();

                while (data_from_querry.Read())
                {
                    Disk_space_used = Math.Round(data_from_querry.GetDouble(0) / 1024, 2);
                    Space_available = Math.Round(data_from_querry.GetDouble(1) / 1024, 2);
                }
                connection_name.Close();

                if ((Disk_space_used + fileDetails.FileSize) < Space_available)
                {
                    return true;

                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                return false;
            }

        }




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

                string querry = "INSERT INTO `File_Table` (`ID`, `User_ID`, `File_name`, " +
                    "`File_Size`, `File_type` , `Date_of_Transfer`, `Source_IP_Adress`) VALUES(@fileID," +
                    " @userID, @fileName, @fileSize, @fileType, @dateTransfer, @sourceIP)";
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

        private void CreateFileID(FileDetails fileDetails)
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
            }
            catch (Exception ex)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Błąd podczas tworzenia folderu użytkownika: " + ex.Message);
                return;
            }
        }


     
        public string CreateFileRespon(FileDetails fileDetails)
        {
            try
            {
                string responString = "/user/User" + fileDetails.userID + "/" + fileDetails.FileID + fileDetails.FileType;
                return responString;
            }
            catch (Exception ex)
            {
                string error = "error";
                Console.WriteLine(ex.Message);
                return error;
            }

        }


        public List<FileDetails> GetFileList(List<FileDetails> listOfFiles, int userID)
        {
            try
            {
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
                        FileSize = data_from_querry.GetInt32(1),
                        FileType = data_from_querry.GetString(2),
                        DateOfTransfer = data_from_querry.GetDateTime(3),
                     
                    };
                    listOfFiles.Add(feld);

                }
                connection_name.Close();


                return listOfFiles;
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Błąd pobiernaia listy plików:" + ex.Message);
                connection_name.Close();
                listOfFiles = null;
                return listOfFiles;
            }
        }






        public string GetFileListRespon(List<FileDetails> listOfFiles)
        {
            string respon = "";


            foreach (FileDetails file in listOfFiles)
            {

                respon = respon + file.FileName + " " + file.FileSize + " " + file.FileType + " " + file.DateOfTransfer + ",";

            }


            return respon;
        }



        public bool IsFileExist(string fileName, int userID, FileDetails fileDetails)
        {
            if (IsFileExistDB(fileName, userID, fileDetails) && IsFileExistOnServer(userID, fileDetails))
            {
                return true;

            }
            else
            {

                DeleteFileOnServer(userID, fileDetails);
                DeleteFileInDB(fileName, userID, fileDetails);

                return false;
            }

        }


        private bool IsFileExistDB(string fileName, int userID, FileDetails fileDetails)
        {
            int result = 0;
            try
            {
                string querry = "SELECT COUNT(ID) FROM `View_Files_Donwload` WHERE File_name LIKE @fileName  AND User_ID= @userID;";
                string querry2 = "SELECT * FROM `View_Files_Donwload` WHERE File_name LIKE @fileName2  AND User_ID= @userID2;";

                MySqlCommand command = new MySqlCommand(querry, connection_name);
                command.Parameters.AddWithValue("@fileName", fileName);
                command.Parameters.AddWithValue("@userID", userID);
                connection_name.Open();
                MySqlDataReader data_from_querry = command.ExecuteReader();

                

                while (data_from_querry.Read())
                {
                    result = data_from_querry.GetInt32(0);
                }
                connection_name.Close();
               
                if (result == 1)
                {
                    MySqlCommand command2 = new MySqlCommand(querry2, connection_name);
                    command2.Parameters.AddWithValue("@fileName2", fileName);
                    command2.Parameters.AddWithValue("@userID2", userID);
                    connection_name.Open();
                    MySqlDataReader data_from_querry2 = command2.ExecuteReader();

              

                    while (data_from_querry2.Read())
                    {
                        fileDetails.FileID = data_from_querry2.GetString(0);
                        fileDetails.userID = data_from_querry2.GetInt32(1);
                        fileDetails.FileName = data_from_querry2.GetString(2);
                        fileDetails.FileType = data_from_querry2.GetString(3);
                    }

                    connection_name.Close();
                    return true;
                }
                else
                {

                    if (IsFileExistOnServer(userID, fileDetails))
                    {
                        DeleteFileOnServer(userID, fileDetails);
                    }

                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd weryfikacji istnienia pliku: " + fileName + "\n" + ex.Message);
                return false;

            }
        }

        private bool IsFileExistOnServer(int userID, FileDetails fileDetails)
        {
            string userFilePath = filePath + "\\User" + userID + "\\" + fileDetails.FileID +  fileDetails.FileType;
            try
            {
                if (File.Exists(userFilePath))
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas sprawdzania, czy plik istnieje na dysku!\n"+ex.ToString());
                return false;
            }
        }

        private void DeleteFileOnServer(int userID, FileDetails fileDetails)
        {
            string userFilePath = filePath + "\\User" + userID + "\\" + fileDetails.FileID + "." + fileDetails.FileType;
            try
            {
                if (File.Exists(userFilePath))
                {
                    File.Delete(userFilePath);
                    
                }
 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas usuwania plików z serwera!");

            }
        }

        private void DeleteFileInDB(string fileName, int userID, FileDetails fileDetails)
        {
            try
            {
                    string querry = "DELETE FROM `File_Table` WHERE `File_Table`.`User_ID`" +
                    " = @userID AND `File_Table`.`File_name` LIKE @fileName AND `File_Table`.`ID` LIKE @fileID;";

                    MySqlCommand command = new MySqlCommand(querry, connection_name);
                  
                    command.Parameters.AddWithValue("@userID", userID);
                    command.Parameters.AddWithValue("@fileName", fileName);
                    command.Parameters.AddWithValue("@fileID", fileDetails.FileID);

                    connection_name.Open();
                    MySqlDataReader data_from_querry = command.ExecuteReader();

            }catch(Exception ex)
            {
                Console.WriteLine("Błąd usuwania wpisu o pliku w DB" + ex.ToString());
            }
        }


        public void Delete(string fileName, int userID, FileDetails fileDetails)
        {
            try {
                DeleteFileInDB(fileName, userID, fileDetails);
                DeleteFileOnServer(userID, fileDetails);
            }catch(Exception ex)
            {
                Console.WriteLine("Błąd usuwania pliku!"+ex.ToString());
            }
        }

    }
}
