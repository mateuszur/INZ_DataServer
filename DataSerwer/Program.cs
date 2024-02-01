using DataServer;

namespace DataSerwer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket_For_Data_Transmission socet = new Socket_For_Data_Transmission();
            socet.Server_Data_Transmission_ListnerAsync();
        }
    }
}
