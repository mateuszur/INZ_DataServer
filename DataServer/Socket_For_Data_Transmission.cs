using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DataServerService
{
    public partial  class Socket_For_Data_Transmission
    {
        private int port;
        TcpListener server;


        public void Server_Data_Transmission_Init()
        {
            this.port = 3333;
            this.server = null;
        }


        public void Server_Data_Transmission_Listner() { 

         try
        {
            server = new TcpListener(IPAddress.Any, port);
        server.Start();
            Console.WriteLine("Serwer nasłuchuje na porcie " + port);

            while (true)
            {
                Console.WriteLine("Oczekiwanie na połączenie...");
                TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Połączono z klientem.");

                // Odbieranie pliku od klienta
                using (NetworkStream stream = client.GetStream())
                {
                    // Odczyt rozmiaru pliku
                    byte[] fileSizeBytes = new byte[4];
        stream.Read(fileSizeBytes, 0, 4);
                    int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

        // Odczyt samego pliku
        byte[] fileData = new byte[fileSize];
        int bytesRead = stream.Read(fileData, 0, fileData.Length);

        // Zapis pliku na serwerze
        string fileName = "received_file.mp4"; // Nazwa pliku na serwerze
        File.WriteAllBytes(fileName, fileData);
                    Console.WriteLine("Odebrano plik: " + fileName);
                }
    client.Close();
            }
        }
        catch (Exception e)
        {
    Console.WriteLine("Błąd: " + e.Message);
}
        finally
        {
    server?.Stop();
}
    }

    }
}
