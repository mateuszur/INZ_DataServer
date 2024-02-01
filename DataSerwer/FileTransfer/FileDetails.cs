using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSerwer.FileTransfer
{
    public class FileDetails
    {

        public string FileID { get; set; }
        public string FileName { get; set; }
        public double FileSize { get; set; }
        public string FileType { get; set; }

        public int userID { get; set; }
        public DateTime DateOfTransfer { get; set; }
        public string SourceIPAddress { get; set; }
    }
}
