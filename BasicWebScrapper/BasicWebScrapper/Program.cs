using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    class Program
    {
        private static string _bestBuyURL;
        private static DateTime startTime;
        private static DateTime endTime;

        // Private variables to hold computer possible information
        private static string[] _availableComputerBrands;
        private static string[] _availableComputerCPU;
        private static string[] _availableComputerGPU;
        private static string[] _availableComputerRAM;
        private static string[] _availableComputerStorage;

        static void Main(string[] args)
        {
            startTime = DateTime.Now;
            InitializeVariables();
            var test = GetComputersFromBestBuy();
            endTime = DateTime.Now;
            Console.WriteLine(endTime - startTime);
            //Console.WriteLine("Hello World!");
        }

        // Method that takes no parameters and scrapes best buy desktop computers
        static List<Computer> GetComputersFromBestBuy()
        {
            // Declare and initialize html web, and load the first page of the best buy URL
            HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load(_bestBuyURL);

            // First page is already loaded so start at 1
            var currentPage = 1;

            // If "page-item" can't be found, assume there is only one page so parse "1" and assign to numberOfPages
            Int32.TryParse(doc.DocumentNode.SelectNodes("//li[contains(@class,'page-item')]").Last().InnerText ?? "1", out int numberOfPages);

            // If number of pages equals 0, assign it the same number as currentPage otherwise assign to itself
            numberOfPages = numberOfPages == 0 ? currentPage : numberOfPages;

            List<Computer> computers = new List<Computer>();
            
            while (currentPage <= numberOfPages)
            {
                // Html nodes that contain all the computer information
                var rightColumnClassHtmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'right-column')]");

                // computerSpecs[1].ToList()[1].InnerText = User Rating
                // computerSpecs[1].ToList()[0].InnerText = Openbox option
                // Html node list that contains brand, model, cpu, gpu, ram, and storage
                var computerInfoHtmlNodeList = rightColumnClassHtmlNodes.Select(node => node.Descendants("a").Where(node => node.GetAttributeValue("href", "").Contains("/site/"))).ToList();

                // Html node list that contains SKU and Model Number
                var computerModelSkuHtmlNodeList = rightColumnClassHtmlNodes.Select(node => node.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("sku-value"))).ToList();

                // Html node list that contains the prices
                var computerPriceHtmlNodeList = rightColumnClassHtmlNodes.Select(node => node.Descendants("span").Where(node => node.GetAttributeValue("aria-hidden", "").Equals("true"))).ToList();

                // Go through each of the node lists
                for (int i = 0; i < computerInfoHtmlNodeList.Count; i++)
                {
                    // Only try to create a computer object and add it to the list of the node lists aren't empty
                    if (computerInfoHtmlNodeList[i].ToList().Count > 0 && computerModelSkuHtmlNodeList[i].ToList().Count > 0 && computerPriceHtmlNodeList[i].ToList().Count > 0)
                    {
                        var computerBrandAndHardware = computerInfoHtmlNodeList[i].ToList()[0].InnerText;
                        computers.Add(new Computer()
                        {
                            // include title in case string parse goes funny
                            Title = computerBrandAndHardware,
                            Brand = getComputerInformation(computerBrandAndHardware, _availableComputerBrands),
                            Model = "",
                            CPU = getComputerInformation(computerBrandAndHardware, _availableComputerCPU),
                            GPU = getComputerInformation(computerBrandAndHardware, _availableComputerGPU),
                            RAM = getComputerInformation(computerBrandAndHardware, _availableComputerRAM),
                            Storage = getComputerInformation(computerBrandAndHardware, _availableComputerStorage),
                            ModelNumber = computerModelSkuHtmlNodeList[i].ToList()[0].InnerText,
                            SKU = computerModelSkuHtmlNodeList[i].ToList()[1].InnerText,
                            Cost = computerPriceHtmlNodeList[i].ToList()[1].InnerText,
                            Link = computerInfoHtmlNodeList[i].ToList()[0].Attributes["href"].Value,
                        });
                    }
                }

                // Increment page and assign the next loaded page to doc
                currentPage++;
                doc = web.Load("https://www.bestbuy.com/site/desktop-computers/all-desktops/pcmcat143400050013.c?cp=" + currentPage + "id=pcmcat143400050013");
            }

            return computers;
        }

        // Method to check if any items in the given array of strings is in the given string, and returns "N/A" if it can't be found
        static string getComputerInformation(string stringToSearch, string[] listOfInfo)
        {
            for (int i = 0; i < listOfInfo.Length; i++)
            {
                if (stringToSearch.Contains(listOfInfo[i])) { return listOfInfo[i]; }
            }
            return "N/A";
        }

        // Method to initialize private variables
        static void InitializeVariables()
        {
            _bestBuyURL = "https://www.bestbuy.com/site/desktop-computers/all-desktops/pcmcat143400050013.c?id=pcmcat143400050013";

            _availableComputerBrands = new string[] { "Acer", "Alienware", "Apple", "ASUS", "Azulle", "CanaKit", "CLX", "CORSAIR", "CyberPowerPC", 
                "CybertronPC", "Dell", "HP", "HP OMEN", "iBUYPOWER", "Intel", "LenovoL", "Microsoft", "MSI", "OptiPlex", "Raspberry Pi", "Shuttle", 
                "Skytech Gaming", "Thermaltake" };
            _availableComputerCPU = new string[] { "Intel Core i3", "Intel Core i5", "Intel Core i7", "Intel Core i9", "AMD Ryzen 3", "AMD Ryzen 5", 
                "AMD Ryzen 7", "AMD Ryzen 9", "AMD Threadripper", "Not Applicable", "Intel Xeon", "Intel Celeron", "Apple M1", "Intel Pentium", 
                "Intel Core 2 Duo", "AMD A-Series A4", "AMD A-Series A9", "AMD A-Series A6", "AMD Athlon Silver 3000 Series", "Intel Core2 Duo" };
            _availableComputerGPU = new string[] { };
            _availableComputerRAM = new string[] { "192GB", "128GB", "96GB", "64GB", "48GB", "32GB", "16GB", "12GB", "8GB", "6GB", "2GB", "512MB" };
            _availableComputerStorage = new string[] { "64GB", "128GB", "256GB", "480GB", "512GB", "1TB", "2TB" };
        }

        // Method to get app settings information
        static void GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
        }
    }
}
