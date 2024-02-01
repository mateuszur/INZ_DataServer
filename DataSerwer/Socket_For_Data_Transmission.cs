using DataSerwer.Configuration;
using DataSerwer.FileTransfer;
using DataSerwer.SessionManager;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Security.Cryptography;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace DataServer
{
    public class Socket_For_Data_Transmission
    {
        // konfiguracja do zaczytania z pliku
        private int dataServerPort = 0;
        private string ftpServerPort = "";
        private string ftpUsername = "";
        private string ftpPassword = "";
        private string filePath = "";
       
        private byte[] key;
        private byte[] iv;

        ReadWriteConfig configReadWrite = new ReadWriteConfig();
        Config config = new Config();

        //Baza danych
        private string connection_string;

        //Połączenie do bazy
         static ParametrFileManager fileManager = new ParametrFileManager();
         static MySqlConnection connection_name = new MySqlConnection();


        TcpListener server;

        //Sesja
        private SessionManager sessionManager = new SessionManager();

        //Timer
        static System.Timers.Timer aTimer;


        public Socket_For_Data_Transmission()
        {

            connection_string = fileManager.ReadParameter();
            connection_name.ConnectionString = connection_string;

            configReadWrite.ReadConfiguration(config);
            this.dataServerPort = config.DataServerPort;
            this.ftpServerPort = config.FTPServerPort;
            this.ftpUsername = config.FTPUsername;
            this.ftpPassword = config.FTPPassword;
            this.filePath = config.FilePath;
            this.key = StringToByteArray(config.Key);
            this.iv= StringToByteArray(config.IV);

            Server_Data_Transmission_ListnerAsync();
        }





        public async Task Server_Data_Transmission_ListnerAsync()
        {
            try
            {
                //Utworzenie obiektu timer na potrzeby sprawdzania stanu sesji w bazie
                aTimer = new System.Timers.Timer(120000);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                //Utworzenie obiektu TcpListener.
                server = new TcpListener(IPAddress.Any, dataServerPort);
                // Start serwera
                server.Start();
                Console.WriteLine("Serwer jest uruchomiony. Oczekiwanie na połączenia...");
                while (true)
                {
                    //Akceptacja połączenia od klienta.
                    TcpClient client = server.AcceptTcpClient();
                    //Pobranie adresu IP klienta
                    IPEndPoint remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    //Utworzenie, odebranie oraz rozszyfrownie danych z  obieku NetworkStream.
                    NetworkStream stream = client.GetStream();
                    byte[] data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);
                    string responseData = Encoding.ASCII.GetString(data, 0, bytes);
                    responseData = DecryptStringFromBytes_Aes(Convert.FromBase64String(responseData), key, iv);
                    //Ukrycie danych logownia oraz polecenia Ping w konsoli
                    if (responseData != "Ping" || !responseData.StartsWith("Login"))
                    {
                        Console.WriteLine("Odebrano: {0}", responseData);
                    }
                    // Odpowiedź dotycząca stanu serwera
                    if (responseData == "Ping")
                    {

                        string plaintext = "Pong";
                        string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                        byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);
                        stream.Write(msg, 0, msg.Length);

                        client.Close();
                    }
                    //Logowanie
                    if (responseData.StartsWith("Login"))
                    {
                        Console.WriteLine(" Otrzymano prośbę o login");
                        string[] parts = responseData.Split(' ');

                        //Sesja
                        SessionManager sessionManager = new SessionManager();

                        if (parts.Length == 3 && sessionManager.IsValidUser(parts[1], parts[2]))
                        {

                            if (sessionManager.IsSessionCreated(DateTime.Now))
                            {
                                string plaintext = "LoginSuccessful," + sessionManager.SessionRespon();
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);
                                stream.Write(msg, 0, msg.Length);


                                Console.WriteLine(" " + DateTime.Now + " Logowanie " + parts[1]);

                            }
                            else
                            {
                                string plaintext = "Sesion created failed";
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                                stream.Write(msg, 0, msg.Length);

                            }
                            client.Close();

                        }
                        else
                        {
                            string plaintext = "Login failed";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            client.Close();
                        }
                    }
                    if (responseData.StartsWith("Logout"))
                    {
                        Console.WriteLine(" Otrzymano prośbę o wylogowanie");
                        string[] parts = responseData.Split(' ');
                        if (parts.Length == 2 && sessionManager.EndSession(parts[1]))
                        {
                            string plaintext = "Logout successful";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                    
                            stream.Write(msg, 0, msg.Length);
                            client.Close();
                        }
                        else
                        {
                            string plaintext = "Logout failed";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                        
                            stream.Write(msg, 0, msg.Length);
                            client.Close();
                        }
                    }
                    //Treansfer plików
                    if (responseData.StartsWith("Upload"))
                    {
                        FileTransferManager fileTransferManager = new FileTransferManager();
                        FileDetails fileDetails = new FileDetails();
                        Console.WriteLine(" Otrzymano prośbę o przesłanie pliku...");
                        string[] parts = responseData.Split(' ');
                       
                        bool result = fileTransferManager.IsSessionValid(parts[1], int.Parse(parts[2]));
                        //Tworzenie pliku w bazie jeżeli pochodzi z obecnej sesji

                        IPAddress clientIpAddress = remoteEndPoint.Address;

                        if (parts.Length == 6 && result)
                        {

                            fileDetails.userID = int.Parse(parts[2]);
                            fileDetails.FileName = parts[3];
                            fileDetails.FileType = parts[4];
                            if (parts.Length > 5 && double.TryParse(parts[5], out double fileSize))
                            {
                                fileDetails.FileSize = Math.Round(fileSize, 3);
                            }
                            else
                            {
                                fileDetails.FileSize = 0.001;
                            }
                            fileDetails.DateOfTransfer = DateTime.Now;
                            fileDetails.SourceIPAddress = clientIpAddress.ToString();

                            if (fileTransferManager.HasUserFreeSpace(fileDetails))
                            {
                                //przygotowujemy lokalziację oraz wpis w bazie 
                                fileTransferManager.CreateFile(fileDetails);

                                string plaintext = "YourPath " + fileTransferManager.CreateFileRespon(fileDetails) + " " + ftpUsername + " " + ftpPassword;
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                                stream.Write(msg, 0, msg.Length);
                                Console.WriteLine(" " + DateTime.Now + " Przesłano ścieżkę do pliku dla użytkownika od ID: " + parts[2] + " Źródłowy adres IP: " + clientIpAddress.ToString());
                                client.Close();

                            }
                            else
                            {
                                //respon o braku miejsca 

                                string plaintext = "NoFreeSpace";
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                                stream.Write(msg, 0, msg.Length);
                                Console.WriteLine(" " + DateTime.Now + " Brak miejsca na plik dla użytkownika o ID: " + parts[2]);
                                client.Close();
                            }
                        }
                        else
                        {

                            string plaintext = "SessionIsNotValid";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Unieważniono sesję użytkonika o ID:" + parts[2]);
                            client.Close();
                        }

                    }
                    if (responseData.StartsWith("Download"))
                    {
                        FileTransferManager fileTransferManager = new FileTransferManager();
                        FileDetails fileDetails = new FileDetails();
                        IPAddress clientIpAddress = remoteEndPoint.Address;

                        Console.WriteLine(" Otrzymano prośbę o pobranie pliku użytkownika.");
                        string[] parts = responseData.Split(' ');

                        if (parts.Length == 4 && fileTransferManager.IsSessionValid(parts[1], int.Parse(parts[2])))
                        {
                            if (fileTransferManager.IsFileExist(parts[3], int.Parse(parts[2]), fileDetails))
                            {
                                string plaintext = "YourPathToDownload " + fileTransferManager.CreateFileRespon(fileDetails) + " " + ftpUsername + " " + ftpPassword;
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);
                                

                                stream.Write(msg, 0, msg.Length);

                                Console.WriteLine(" " + DateTime.Now + " Przesłano ścieżkę do pliku dla użytkownika od ID: " + parts[2] + " Źródłowy adres IP: " + clientIpAddress.ToString());
                                client.Close();
                            }
                            else
                            {
                                string plaintext = "FileDoesntExist";
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                                stream.Write(msg, 0, msg.Length);
                                Console.WriteLine(" " + DateTime.Now + " Nie odnaleziponou pliku o nazwie: " + parts[3]);
                                client.Close();
                            }
                        }
                        else
                        {
                            string plaintext = "SessionIsNotValid";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Unieważniono sesję użytkonika o ID:" + parts[2]);
                            client.Close();
                        }

                    }
                    if (responseData.StartsWith("Delete"))
                    {
                        FileTransferManager fileTransferManager = new FileTransferManager();
                        FileDetails fileDetails = new FileDetails();
                        IPAddress clientIpAddress = remoteEndPoint.Address;

                        Console.WriteLine(" Otrzymano prośbę o usunięcie pliku użytkownika.");
                        string[] parts = responseData.Split(' ');

                        if (parts.Length == 4 && fileTransferManager.IsSessionValid(parts[1], int.Parse(parts[2])))
                        {
                            if (fileTransferManager.IsFileExist(parts[3], int.Parse(parts[2]), fileDetails))
                            {

                                fileTransferManager.Delete(parts[3], int.Parse(parts[2]), fileDetails);

                                string plaintext = "FileDeletedSuccessfully";
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                                stream.Write(msg, 0, msg.Length);

                                Console.WriteLine(" " + DateTime.Now + " Usuniętop plik użytkownika o ID: " + parts[2] + " Źródłowy adres IP: " + clientIpAddress.ToString());
                                client.Close();
                            }
                            else
                            {
                                string plaintext = "FileDoesntExist";
                                string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                                byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                                stream.Write(msg, 0, msg.Length);
                                Console.WriteLine(" " + DateTime.Now + " Nie odnaleziponou pliku o nazwie: " + parts[3]);
                                client.Close();
                            }
                        }
                        else
                        {
                            string plaintext = "SessionIsNotValid";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Unieważniono sesję użytkonika o ID:" + parts[2]);
                            client.Close();
                        }

                    }
                    if (responseData.StartsWith("List"))
                    {
                        FileTransferManager fileTransferManager = new FileTransferManager();
                        List<FileDetails> fileDetailsList = new List<FileDetails>();

                        Console.WriteLine(" Otrzymano prośbę o przesłanie listy plików użytkownika.");
                        string[] parts = responseData.Split(' ');

                        if (parts.Length == 3 && fileTransferManager.IsSessionValid(parts[1], int.Parse(parts[2])))
                        {

                            fileTransferManager.GetFileList(fileDetailsList, int.Parse(parts[2]));

                            string plaintext = "FileList," + fileTransferManager.GetFileListRespon(fileDetailsList);
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);


                            Console.WriteLine(" " + DateTime.Now + " Przesłano listę plików użytkownika ID: " + parts[2]);
                            client.Close();

                        }
                        else
                        {
                            string plaintext = "SessionIsNotValid";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Unieważniono sesję użytkonika o ID:" + parts[2]);
                            client.Close();
                        }



                    }
                    //Weryfiakcja sesji
                    if (responseData.StartsWith("IsSessionValid"))
                    {
                        Console.WriteLine(" Otrzymano prośbę o  weryfiakcję ważnosci sesji klienta...");

                        string[] parts = responseData.Split(' ');
                        SessionManager sessionManager = new SessionManager();

                        if (sessionManager.IsSessionValid(parts[1], parts[2]))
                        {

                            string plaintext = "Sesion is valid";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            client.Close();

                        }
                        else
                        {
                            string plaintext = "Sesion is not valid";
                            string encryptedMessage = Convert.ToBase64String(EncryptStringToBytes_Aes(plaintext, key, iv));
                            byte[] msg = Encoding.ASCII.GetBytes(encryptedMessage);

                            stream.Write(msg, 0, msg.Length);
                            client.Close();

                        }
                    }
                    //Zatrzymanie serwera
                    if (responseData.StartsWith("STOP") && remoteEndPoint.Address.Equals(IPAddress.Loopback))
                    {
                        Console.WriteLine("Zamykanie serwera...");
                        client.Close();
                        server.Stop();
                        break;
                    }

                    client.Close();
                }
                await Task.Delay(1500);
                System.Environment.Exit(0);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }


        //obsługa wykonywania czasu czyszczenia bazy z sesji starszych jak 24h
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DBclean();
        }


        private static void DBclean()
        {
            try
            {
                connection_name.Open();

                string sqlQery = "UPDATE `Sesion` SET `Active` = 0 WHERE `End_Sesion_Date` < @dateNow  AND `Active` = 1;";

                MySqlCommand command = new MySqlCommand(sqlQery, connection_name);
                command.Parameters.AddWithValue("@dateNow", DateTime.Now);

                command.ExecuteNonQuery();

                connection_name.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "\n");
            }
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
