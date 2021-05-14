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
        private static IConfiguration _configuration;
        private static IHttpClientFactory _httpClientFactory;
        private static string _bestBuyURL;

        private static string[] _availableComputerBrands;
        private static string[] _availableComputerCPU;
        private static string[] _availableComputerGPU;
        private static string[] _availableComputerRAM;
        private static string[] _availableComputerStorage;

        static void Main(string[] args)
        {
            InitializeVariables();
            GetComputersFromBestBuy();

            //Console.WriteLine("Hello World!");
        }

        static void GetComputersFromBestBuy()
        {
            HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load(_bestBuyURL);

            var rightColumnClassHtmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'right-column')]");

            // computerSpecs[1].ToList()[1].InnerText = User Rating
            // computerSpecs[1].ToList()[0].InnerText = Openbox option
            // Html node list that contains brand, model, cpu, gpu, ram, and storage
            var computerInfoHtmlHodeList = rightColumnClassHtmlNodes.Select(node => node.Descendants("a").Where(node => node.GetAttributeValue("href", "").Contains("/site/"))).ToList();

            // Html node list that contains SKU and Model Number
            var computerModelSkuHtmlNodeList = rightColumnClassHtmlNodes.Select(node => node.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("sku-value"))).ToList();

            // Html node list that contains the price
            var computerPriceHtmlNodeList = rightColumnClassHtmlNodes.Select(node => node.Descendants("span").Where(node => node.GetAttributeValue("aria-hidden", "").Equals("true"))).ToList();

            List <Computer> computers = new List<Computer>();

            for (int i = 0; i < computerInfoHtmlHodeList.Count; i++)
            {
                if (computerInfoHtmlHodeList[i].ToList().Count > 0 && computerModelSkuHtmlNodeList[i].ToList().Count > 0 && computerPriceHtmlNodeList[i].ToList().Count > 0)
                {
                    var computerBrandAndHardware = computerInfoHtmlHodeList[i].ToList()[0].InnerText;
                    computers.Add(new Computer()
                    {
                        Title = computerInfoHtmlHodeList[i].ToList()[0].InnerText
                        Brand = "",
                        Model = "",
                        CPU = "",
                        GPU = "",
                        RAM = "",
                        Storage = "",
                        ModelNumber = computerModelSkuHtmlNodeList[i].ToList()[0].InnerText,
                        SKU = computerModelSkuHtmlNodeList[i].ToList()[1].InnerText,
                        Cost = computerPriceHtmlNodeList[i].ToList()[1].InnerText,
                        Link = computerInfoHtmlHodeList[i].ToList()[0].Attributes["href"].Value,
                    });
                }                
            }
        }

        static void InitializeVariables()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

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

        static void GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
        }
    }
}
