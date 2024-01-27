using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSerwer.Configuration
{
    class ParametrFileManager
    {
        private string filePath;

        public ParametrFileManager()
        {
            filePath = "..\\Config\\ConnectionString.txt";
        }
        public void SaveParameter(string parameter)
        {
            File.WriteAllText(filePath, parameter);
        }

        public string ReadParameter()
        {
            return File.ReadAllText(filePath);
        }

    }
}
