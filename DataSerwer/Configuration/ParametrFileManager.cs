using System.IO;

namespace DataServer.Configurations
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
