using System.IO;

namespace BasicWebScrapper
{
    class HelperMethods
    {
        // Method to check if the file exists
        private bool DoesFileExist(string fileName, string fileTypeExtension)
        {
            return File.Exists($"{Directory.GetCurrentDirectory()}\\{fileName}{fileTypeExtension}");
        }

        // Method to check if a file name is available at the current directory and return an available string name
        public string GetAvailableFileName(string fileName, string fileTypeExtension)
        {
            var fileExists = DoesFileExist(fileName, fileTypeExtension);

            if (fileExists)
            {
                return $"{fileName}{fileTypeExtension}";
            }

            int i = 0;
            while (DoesFileExist($"{fileName}_{i}", fileTypeExtension))
            {
                i++;
            }

            return $"{fileName}_{i}{fileTypeExtension}";
        }

        public string CreateCurrentDirectoryPath(string fileName)
        {
            return $"{Directory.GetCurrentDirectory()}\\{fileName}.xlsx";
        }
    }
}
