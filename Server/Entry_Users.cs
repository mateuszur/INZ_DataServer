using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Entry_Users
    {

        public int ID { get; set; }
        public string Login { get; set; }
        public string Privileges { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Space_available { get; set; }

        public double Disk_space_used { get; set; }
    }
}
