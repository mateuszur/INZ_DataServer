using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServerService
{
    public class FileTransferManager
    {

        private FileDetails fileDetails = new FileDetails();

        //Baza danych do dodania zaczytanie z pliku konfiguracyjnego
        private string connection_string = "Server=192.168.1.51;Uid=inz;Pwd=Pa$$w0rd;Database=Server_inz_MU23/24;";
        private MySqlConnection connection_name = new MySqlConnection();


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
            connection_name.ConnectionString = connection_string;

            string query= "SELECT COUNT(ID) FROM `View_Session` WHERE ID like @sessionID";

         MySqlCommand command=new MySqlCommand(query, connection_name);

            command.Parameters.AddWithValue("@sessionID", sessionID);

            if(command.ExecuteNonQuery()==1) { 
            
                return true;
            }else
            {
                return false;
            }

        }

    }
}
