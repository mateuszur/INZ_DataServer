using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSerwer.Configuration
{
    public class Config
    {
        public int DataServerPort { get; set; } = 3333;
        public string FTPServerPort { get; set; }
        public string FTPUsername { get; set; }
        public string FTPPassword { get; set; }
        public string CertificatePublicKey { get; set; }
        public string CertificatePrivateKey { get; set; }

        public string FilePath { get; set; }
    }
}
