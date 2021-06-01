using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    class Program
    {
        private static string _bestBuyURL;
        private static DateTime startTime;
        private static DateTime endTime;
        private static DateTime endExportTime;
        private static ExcelUtility _excelUtility;
        private static ExtractSpecification _extractSpecification;

        static void Main(string[] args)
        {
            startTime = DateTime.Now;
            InitializeVariables();

            // For testing
            //_computerSpecificationDictionary.CheckSpecifications("Intel - NUC 9 Pro NUC9VXQNX Workstation - Xeon E-2286M  UHD Graphics P630 Mini PC - Black");

            var computers = GetComputersFromBestBuy();
            endTime = DateTime.Now;
            _excelUtility.exportToExcel(computers);
            endExportTime = DateTime.Now;
            Console.WriteLine(endTime - startTime);
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
                        var elementIndex = 0;
                        var computerBrandAndHardware = computerInfoHtmlNodeList[i].ToList()[0].InnerText;
                        if (computerBrandAndHardware == "")
                        {
                            elementIndex = 1;
                            computerBrandAndHardware = computerInfoHtmlNodeList[i].ToList()[1].InnerText;
                        }
                        var computer = _extractSpecification.ExtractFromString(computerBrandAndHardware);
                        computer.Model = computerModelSkuHtmlNodeList[i].ToList().ElementAtOrDefault(0)?.InnerText ?? "N/A";
                        computer.SKU = computerModelSkuHtmlNodeList[i].ToList().ElementAtOrDefault(1)?.InnerText ?? "N/A";
                        computer.Cost = computerPriceHtmlNodeList[i].ToList().ElementAtOrDefault(1)?.InnerText ?? "N/A";
                        computer.Link = computerInfoHtmlNodeList[i].ToList().ElementAtOrDefault(elementIndex)?.Attributes["href"].Value;
                        computers.Add(computer);
                    }
                }

                // Increment page and assign the next loaded page to doc
                currentPage++;
                doc = web.Load("https://www.bestbuy.com/site/desktop-computers/all-desktops/pcmcat143400050013.c?cp=" + currentPage + "&id=pcmcat143400050013");
            }

            return computers;
        }

        // Method to initialize private variables
        static void InitializeVariables()
        {
            _excelUtility = new ExcelUtility();
            _extractSpecification = new ExtractSpecification();

            _bestBuyURL = "https://www.bestbuy.com/site/desktop-computers/all-desktops/pcmcat143400050013.c?id=pcmcat143400050013";
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
