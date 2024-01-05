using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServerService
{
    internal class FileDetails
    {
        public string FileID { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string FileType { get; set; }

        public int userID { get; set; }
        public DateTime DateOfTransfer { get; set; }
        public string SourceIPAddress { get; set; }
    }
}
