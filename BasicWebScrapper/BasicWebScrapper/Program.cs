using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    class Program
    {
        private static string _bestBuyURL;
        private static DateTime startTime;
        private static DateTime endTime;
        private static DateTime endExportTime;
        private static IHttpClientFactory _httpClientFactory;
        private static ExcelUtility _excelUtility;
        private static ExtractSpecification _extractSpecification;
        private static BestBuyComputers _bestBuyComputers;

        static async Task Main(string[] args)
        {
            startTime = DateTime.Now;

            GetConfiguration();
            InitializeVariables();

            // For testing
            //HelperMethods helperMethods = new HelperMethods();
            //var test = helperMethods.DoesFileExist("ComputerList");
            //var test2 = helperMethods.CheckAndGetFileName("ComputerList");
            //var computers = await _bestBuyComputers.GetDesktopInformationFromPage("site/desktop-computers/all-desktops/pcmcat143400050013.c?id=pcmcat143400050013", true);
            //var test = _extractSpecification.ExtractFromString("ASUS - M241DA 23.8&#x27;&#x27; Touch-Screen All-In-One - AMD R5-3500U - 8GB Memory - 256GB Solid State Drive - Black - Black");

            var computers = await _bestBuyComputers.GetDesktopComputers();            

            endTime = DateTime.Now;
            _excelUtility.AddComputersToWorkbook("BestBuy - Desktops", _bestBuyURL, computers);
            _excelUtility.ExportWorkbooksToExcel();
            endExportTime = DateTime.Now;
            Console.WriteLine($"Get Computers: {endTime - startTime}\nExport: {endExportTime - endTime}");
        }

        // Method to initialize private variables
        static void InitializeVariables()
        {
            _bestBuyURL = "https://www.bestbuy.com";

            var serviceProvider = new ServiceCollection().AddHttpClient("BestBuy", c =>
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


            //var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider()

            //var service = serviceProvider.Services.AddHttpClient().BuildServiceProvider();

            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            _excelUtility = new ExcelUtility();
            _extractSpecification = new ExtractSpecification();
            _bestBuyComputers = new BestBuyComputers(_httpClientFactory);
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
