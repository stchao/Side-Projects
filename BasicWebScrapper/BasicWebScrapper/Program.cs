using BasicWebScrapper.Sites;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    class Program
    {
        private static string _bestBuyURL;
        private static string _officeDepotURL;
        private static DateTime startTime;
        private static DateTime endTime;
        private static DateTime endExportTime;
        private static IHttpClientFactory _httpClientFactory;
        private static ExcelUtility _excelUtility;
        private static BestBuy _bestBuyComputers;
        private static OfficeDepot _officeDepotComputers;
        private static List<LogMessage> _errors;
        private static string _bestBuyDesktopComputersFirstPagePath;
        private static string _bestBuyDesktopComputersFollowingPagePath;
        private static string _bestBuyLaptopComputersFirstPagePath;
        private static string _bestBuyLaptopComputersFollowingPagePath;

        static async Task Main()
        {
            startTime = DateTime.Now;

            GetConfiguration();
            InitializeVariables();

            // For testing
            //LogMessage log = new LogMessage { LogDate = DateTime.Now, Message = "testing" };
            //List<LogMessage> logs = new List<LogMessage>() { log, log, log };
            //_excelUtility.AddLogsToWorkbook("Logs", logs);
            //_excelUtility.AddLogsToWorkbook("Errors", logs);
            //_excelUtility.ExportWorkbooksToExcel();
            //HelperMethods helperMethods = new HelperMethods();
            //var test = helperMethods.DoesFileExist("ComputerList");
            //var test2 = helperMethods.CheckAndGetFileName("ComputerList");
            var computers = await _officeDepotComputers.GetInformationFromPage("desktop-computers/N=5+1461989&viewAllSkus=true&recordsPerPageNumber=72&No=0/", false, "Desktop");
            endTime = DateTime.Now;
            _excelUtility.AddComputersToWorkbook("New", _bestBuyURL, _officeDepotComputers.NewComputers);
            _excelUtility.AddComputersToWorkbook("Open-Box", _bestBuyURL, _officeDepotComputers.OpenBoxComputers);
            _excelUtility.AddComputersToWorkbook("Refurbished", _bestBuyURL, _officeDepotComputers.RefurbishedComputers);
            _excelUtility.AddComputersToWorkbook("Unavailable", _bestBuyURL, _officeDepotComputers.UnavailableComputers);
            _excelUtility.AddLogsToWorkbook("Logs", new List<LogMessage>());

            _errors.AddRange(_bestBuyComputers.Errors);
            _errors.AddRange(_excelUtility.Errors);
            _excelUtility.AddLogsToWorkbook("Errors", _errors);

            _excelUtility.ExportWorkbooksToExcel();
            endExportTime = DateTime.Now;
            Console.WriteLine($"Get Computers: {endTime - startTime}\nExport: {endExportTime - endTime}");

            //var test = _extractSpecification.ExtractFromString("ASUS - M241DA 23.8&#x27;&#x27; Touch-Screen All-In-One - AMD R5-3500U - 8GB Memory - 256GB Solid State Drive - Black - Black");

            var bestBuyDesktopComputersTask = _bestBuyComputers.GetComputers(_bestBuyDesktopComputersFirstPagePath, _bestBuyDesktopComputersFollowingPagePath, "Desktop");
            var bestBuyLaptopComputersTask = _bestBuyComputers.GetComputers(_bestBuyLaptopComputersFirstPagePath, _bestBuyLaptopComputersFollowingPagePath, "Laptop");
            await bestBuyDesktopComputersTask;
            await bestBuyLaptopComputersTask;

            endTime = DateTime.Now;
            _excelUtility.AddComputersToWorkbook("New", _bestBuyURL, _bestBuyComputers.NewComputers);
            _excelUtility.AddComputersToWorkbook("Open-Box", _bestBuyURL, _bestBuyComputers.OpenBoxComputers);
            _excelUtility.AddComputersToWorkbook("Refurbished", _bestBuyURL, _bestBuyComputers.RefurbishedComputers);
            _excelUtility.AddComputersToWorkbook("Unavailable", _bestBuyURL, _bestBuyComputers.UnavailableComputers);
            _excelUtility.AddLogsToWorkbook("Logs", new List<LogMessage>());

            _errors.AddRange(_bestBuyComputers.Errors);
            _errors.AddRange(_excelUtility.Errors);
            _excelUtility.AddLogsToWorkbook("Errors", _errors);

            _excelUtility.ExportWorkbooksToExcel();
            endExportTime = DateTime.Now;
            Console.WriteLine($"Get Computers: {endTime - startTime}\nExport: {endExportTime - endTime}");
        }

        // Method to initialize private variables
        static void InitializeVariables()
        {
            _bestBuyURL = "https://www.bestbuy.com";
            _officeDepotURL = "https://www.officedepot.com/a/browse/";

            var serviceProvider = new ServiceCollection();
            
            serviceProvider.AddHttpClient("BestBuy", c =>
            {
                c.BaseAddress = new Uri(_bestBuyURL);
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.DefaultRequestHeaders.Add("User-Agent", "BasicWebScrapper");
                c.DefaultRequestHeaders.Add("Accept", "*/*");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }).Services.BuildServiceProvider();

            serviceProvider.AddHttpClient("OfficeDepot", c =>
            {
                c.BaseAddress = new Uri(_officeDepotURL);
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.DefaultRequestHeaders.Add("User-Agent", "BasicWebScrapper");
                c.DefaultRequestHeaders.Add("Accept", "*/*");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }).Services.BuildServiceProvider();

            //var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider()

            //var service = serviceProvider.Services.AddHttpClient().BuildServiceProvider();

            //_httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            _httpClientFactory = serviceProvider.BuildServiceProvider().GetService<IHttpClientFactory>();

            _errors = new List<LogMessage>();
            _excelUtility = new ExcelUtility();
            _bestBuyComputers = new BestBuy(_httpClientFactory);
            _officeDepotComputers = new OfficeDepot(_httpClientFactory);

            _bestBuyDesktopComputersFirstPagePath = "/site/desktop-computers/all-desktops/pcmcat143400050013.c";
            _bestBuyDesktopComputersFollowingPagePath = "/site/desktop-computers/all-desktops/pcmcat143400050013.c?cp=";
            _bestBuyLaptopComputersFirstPagePath = "/site/desktop-computers/all-desktops/pcmcat143400050013.c";
            _bestBuyLaptopComputersFollowingPagePath = "/site/desktop-computers/all-desktops/pcmcat143400050013.c?cp=";
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
