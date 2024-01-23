using System.IO;

namespace Server
{

    class ParametrFileManager
    {
        private string filePath;

        public ParametrFileManager()
        {
            this.filePath = ".\\Config\\ConnectionString.txt";
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
