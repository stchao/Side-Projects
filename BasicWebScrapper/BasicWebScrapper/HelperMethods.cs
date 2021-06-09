using System.IO;

namespace BasicWebScrapper
{
    class HelperMethods
    {
        // Method to check if the file exists
        public bool DoesFileExist(string fileName)
        {
            return File.Exists($"{Directory.GetCurrentDirectory()}\\{fileName}.xlsx");
        }

        // Method to check if a file name is available at the current directory and return an available string name
        public string CheckAndGetFileName(string fileName)
        {
            int i = 0;
            while (DoesFileExist($"{fileName}_{i}"))
            {
                i++;
            }

            return $"{fileName}_{i}.xlsx";
        }
    }
}
