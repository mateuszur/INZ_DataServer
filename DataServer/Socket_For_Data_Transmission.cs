using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DataServerService
{
    public partial  class Socket_For_Data_Transmission
    {



        // konfiguracja do zaczytania z pliku
        private int port;
        string FTP_username = "user";
        string FTP_password = "Pa$$w0rd";



        TcpListener server;

        //Sesja
        private SessionManager sessionManager = new SessionManager();

        //Timer
        static System.Timers.Timer aTimer;


        public void Server_Data_Transmission_Init()
        {
            this.port = 3333;
            this.server = null;
        }


       


        public void Server_Data_Transmission_Listner()
        {
           


            //Baza danych
            string connection_string = "Server=polsl.online;Uid=inz;Pwd=Pa$$w0rd;Database=inz_MU23/24;";
            MySqlConnection connection_name = new MySqlConnection();


            //Połączenie do bazy
            connection_name.ConnectionString = connection_string;

            //Utworzenie obiektu timer na potrzeby sprawdzania stanu sesji w bazie

            aTimer = new System.Timers.Timer(120000);
            // Dodaj zdarzenie Elapsed do timera
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;



            // Utwórz obiekt TcpListener.
            server = new TcpListener(IPAddress.Any, port);

            // Zacznij nasłuchiwać połączeń przychodzących.
            server.Start();

            Console.WriteLine("Serwer jest uruchomiony. Oczekiwanie na połączenia...");
            
            while (true)
            {
                // Akceptuj połączenie od klienta.
                TcpClient client = server.AcceptTcpClient();
                //pobranie adresu IP klienta
                IPEndPoint remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                // Pobierz obiekt NetworkStream.
                NetworkStream stream = client.GetStream();

                // Utwórz obiekt StreamReader do odczytu z NetworkStream.
                StreamReader reader = new StreamReader(stream);


                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Odebrano: {0}", responseData);

                // Odpowiedź
                if (responseData == "Ping")
                {    
                    Console.WriteLine(DateTime.Now.ToString()+ " Otrzymano PING");
                    byte[] msg = Encoding.ASCII.GetBytes("Pong");
                    stream.Write(msg, 0, msg.Length);

                }
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
                            byte[] msg = Encoding.ASCII.GetBytes("LoginSuccessful," + sessionManager.SessionRespon());
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Logowanie " + parts[1]);

                        }
                        else
                        {
                            byte[] msg = Encoding.ASCII.GetBytes("Sesion created failed");
                            stream.Write(msg, 0, msg.Length);

                        }
                        client.Close();

                    }
                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("Login failed");
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
                        byte[] msg = Encoding.ASCII.GetBytes("Logout successful");
                        stream.Write(msg, 0, msg.Length);
                    }
                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("Logout failed");
                        stream.Write(msg, 0, msg.Length);
                    }
                }


                //Treansfer plików
                if (responseData.StartsWith("Upload"))
                {
                    Console.WriteLine(" Otrzymano prośbę o przesłanie pliku...");
                    string[] parts = responseData.Split(' ');

                    //Tworzenie pliku w bazie jeżeli pochodzi z obecnej sesji
                    FileTransferManager fileTransferManager = new FileTransferManager();
                    FileDetails fileDetails = new FileDetails();
                    IPAddress clientIpAddress = remoteEndPoint.Address;
                   
                    if (parts.Length == 6 && fileTransferManager.IsSessionValid(parts[1], int.Parse(parts[2])))
                    {

                        fileDetails.userID = int.Parse(parts[2]);
                        fileDetails.FileName = parts[3];
                        fileDetails.FileType = parts[4];
                        fileDetails.FileSize = int.Parse(parts[5]);
                        fileDetails.DateOfTransfer = DateTime.Now;
                        fileDetails.SourceIPAddress= clientIpAddress.ToString();

                        if (fileTransferManager.HasUserFreeSpace(fileDetails))
                        {
                            //przygotowujemy lokalziację oraz wpis w bazie 
                            fileTransferManager.CreateFile(fileDetails);

                            byte[] msg = Encoding.ASCII.GetBytes("YourPath " + fileTransferManager.CreateFileRespon(fileDetails) + " "+FTP_username+" "+FTP_password);

                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Przesłano ścieżkę do pliku dla użytkownika od ID: " + parts[2] + " Źródłowy adres IP: "+ clientIpAddress.ToString());
                            client.Close();

                        }
                        else
                        {
                            //respon o braku miejsca 
                            byte[] msg = Encoding.ASCII.GetBytes("NoFreeSpace");

                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(" " + DateTime.Now + " Brak miejsca na plik dla użytkownika o ID: "+ parts[2]);
                            client.Close();
                        }
                    }
                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("SessionIsNotValid");

                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine(" " + DateTime.Now + " Unieważniono sesję użytkonika o ID:" + parts[2]);
                        client.Close();
                    }

                }

                if (responseData.StartsWith("Download"))
                {

                }

                if (responseData.StartsWith("List"))
                {
                    FileTransferManager fileTransferManager = new FileTransferManager();
                    List<FileDetails> fileDetailsList = new List<FileDetails>();

                    Console.WriteLine(" Otrzymano prośbę o przesłanie listy plików użytkownika.");
                    string[] parts = responseData.Split(' ');
                    
                    if(parts.Length == 3 && fileTransferManager.IsSessionValid(parts[1], int.Parse(parts[2])))
                    {
                        
                        fileTransferManager.GetFileList(fileDetailsList, int.Parse(parts[2]));

                        byte[] msg = Encoding.ASCII.GetBytes("FileList," + fileTransferManager.GetFileListRespon(fileDetailsList));

                        stream.Write(msg, 0, msg.Length);


                        Console.WriteLine(" " + DateTime.Now + " Przesłano listę plików użytkownika ID: " + parts[2]);
                        client.Close();

                    }
                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("SessionIsNotValid");

                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine(" " + DateTime.Now + " Unieważniono sesję użytkonika o ID:" + parts[2]);
                        client.Close();
                    }

                    

                }

                //Weryfiakcja sesji
                if(responseData.StartsWith("IsSessionValid"))
                {
                    Console.WriteLine(" Otrzymano prośbę o  weryfiakcję ważnosci sesji klienta...");

                    string[] parts = responseData.Split(' ');
                    SessionManager sessionManager = new SessionManager();

                    if (sessionManager.IsSessionValid(parts[1], parts[2]))
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("Sesion is valid");
                        stream.Write(msg, 0, msg.Length);
                        client.Close();

                    }
                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("Sesion is not valid");
                        stream.Write(msg, 0, msg.Length);
                        client.Close();

                    }


                    

                }


                client.Close();

                
            }

        }


        //obsługa wykonywania czasu czyszczenia bazy z sesji starcszych jak 24h
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DBclean();
        }


        private static void DBclean()
        {
            try
            {
                string connection_string = "Server=192.168.1.51;Uid=inz;Pwd=Pa$$w0rd;Database=Server_inz_MU23/24;";
                MySqlConnection connection_name = new MySqlConnection();
                connection_name.ConnectionString = connection_string;
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

    }
}
