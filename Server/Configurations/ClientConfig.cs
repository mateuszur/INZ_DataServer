using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServerGUI.Configurations
{
    public class ClientConfig
    {

        public string ServerAddress { get; set; }
        public int DataServerPort { get; set; } = 3333;
       
        

        public string PublicKey { get; set; }


        
    }
}
