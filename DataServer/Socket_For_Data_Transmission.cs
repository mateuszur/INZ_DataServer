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
        private int port;
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
                            byte[] msg = Encoding.ASCII.GetBytes("LoginSuccessful " + sessionManager.SessionRespon());
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


                if (responseData.StartsWith("Upload"))
                {
                    Console.WriteLine(" Otrzymano prośbę o przesłanie pliku...");
                    string[] parts = responseData.Split(' ');

                    //Tworzenie pliku w bazie jeżeli pochodzi z obecnej sesji
                    FileTransferManager fileTransferManager = new FileTransferManager();

                    if(parts.Length == 4 && fileTransferManager.IsSessionValid(parts[1]))
                    {

                    }

                }

                if (responseData.StartsWith("Download"))
                {

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
