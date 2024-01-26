using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataServer;

namespace DataSerwer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket_For_Data_Transmission socet = new Socket_For_Data_Transmission();
            socet.Server_Data_Transmission_Listner();
        }
    }
}
