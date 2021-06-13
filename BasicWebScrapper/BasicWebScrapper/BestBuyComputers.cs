using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    class BestBuyComputers
    {
        private readonly HttpClient _httpClient;
        private readonly ExtractSpecification _extractSpecification;
        private readonly List<LogMessage> _errors;
        private readonly List<Computer> _newComputers;
        private readonly List<Computer> _openBoxComputers;
        private readonly List<Computer> _refurbishedComputers;
        private readonly List<Computer> _unavailableComputers;

        // private regex variables for extracting information 
        private readonly Regex _computerInformationRegex = new Regex("<div class=\"sku-title\">.*<\\/div>");
        private readonly Regex _computerPriceRegex = new Regex("<span(?: class=\"open-box-lowest-price\")? aria-hidden=\"true\">([$](?:<!-- -->)?\\d{0,2}?,?\\d{0,3}.\\d{0,2})<\\/span>");
        private readonly Regex _lastPageNumberRegex = new Regex("class=\"trans-button page-number\" aria-label=\"Results Page \\d\\d\">(\\d\\d)");
        private readonly Regex _computerAvailabilityRegex = new Regex("(?:style=\"padding:0 8px\">)(.*)(?:<\\/button>|<\\/a>)");

        // private variable to keep track of the number of pages
        private int _lastPageNumber = 1;

        // Initializing private variables
        public BestBuyComputers(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BestBuy");
            _extractSpecification = new ExtractSpecification();
            _errors = new List<LogMessage>();
            _newComputers = new List<Computer>();
            _openBoxComputers = new List<Computer>();
            _refurbishedComputers = new List<Computer>();
            _unavailableComputers = new List<Computer>();            
        }
        public List<LogMessage> Errors { get { return _errors; } }
        public List<Computer> NewComputers { get { return _newComputers; } }
        public List<Computer> OpenBoxComputers { get { return _openBoxComputers; } }
        public List<Computer> RefurbishedComputers { get { return _refurbishedComputers; } }
        public List<Computer> UnavailableComputers { get { return _unavailableComputers; } }

        // Method to get all desktop computers
        public async Task<bool> GetComputers(string firstPagePath, string followPagePaths, string computerType)
        {
            // Get computers from first page only to extract last page
            bool successfullyRetrievedFirstPage = await GetInformationFromPage(firstPagePath, true, computerType);

            if (successfullyRetrievedFirstPage)
            {
                // Declare and create a task for each page up to and including the last page
                List<Task<bool>> successfullyRetrieveOtherPagesTasks = new List<Task<bool>>();
                for (int i = 1; i < _lastPageNumber + 1; i++)
                {
                    successfullyRetrieveOtherPagesTasks.Add(GetInformationFromPage(followPagePaths + i, false, computerType));
                }

                // Wait until all the tasks are complete
                await Task.WhenAll(successfullyRetrieveOtherPagesTasks);

                return true;
            }
            else
            {
                _errors.Add(new LogMessage
                {
                    LogDate = DateTime.Now,
                    Method = "GetDesktopComputers",
                    Parameters = "Path string: " + firstPagePath,
                    Message = "Unable to retrieve first page"
                });
            }
            return false;
        }

        // Method to get the computer information from the page of the path string
        public async Task<bool> GetInformationFromPage(string pathString, bool firstPage, string computerType)
        {
            try
            {
                var response = await _httpClient.GetAsync(pathString);

                // Continue if the 'GET' was successful, indicated by a status code of 'OK'
                if (response.StatusCode == HttpStatusCode.OK)
                {                    
                    var bestBuyHTML = await response.Content.ReadAsStringAsync();

                    // Get last number number if this method is called on the first page
                    if (firstPage)
                    {
                        var lastPageNumberMatch = _lastPageNumberRegex.Match(bestBuyHTML);

                        if (lastPageNumberMatch.Groups.Count > 1)
                        {
                            Int32.TryParse(lastPageNumberMatch.Groups[1].Value, out _lastPageNumber);
                        }
                    }

                    // Variables to hold the computer information and price matches
                    var computerInformationMatches = _computerInformationRegex.Matches(bestBuyHTML);
                    var computerPriceMatches = _computerPriceRegex.Matches(bestBuyHTML);
                    var computerAvailabilityMatches = _computerAvailabilityRegex.Matches(bestBuyHTML);

                    // Go through each of the matches and don't need to do separate for loops since each computer should have a corresponding price
                    for (int i = 0; i < computerInformationMatches.Count; i++)
                    {
                        var computer = new Computer();
                        var computerInformationMatch = computerInformationMatches[i].Value;

                        // Variables to store the link and computer specification indexes
                        var linkStartIndex = computerInformationMatch.IndexOf("<a href=\"") + "<a href=\"".Length;
                        var linkEndIndex = computerInformationMatch.IndexOf("\">", linkStartIndex);
                        var computerSpecificationStartIndex = linkEndIndex + "\">".Length;
                        var computerSpecificationEndIndex = computerInformationMatch.IndexOf("</a>", computerSpecificationStartIndex);

                        // Variables to store the SKU and model indexes
                        var computerSpanHTMLLength = "<span class=\"sku-value\">".Length;
                        var computerModelStartIndex = computerInformationMatch.IndexOf("<span class=\"sku-value\">");
                        var computerSKUStartIndex = computerInformationMatch.IndexOf("<span class=\"sku-value\">", computerModelStartIndex > 0 ? computerModelStartIndex + computerSpanHTMLLength : 0);

                        // Variable to store the extracted computer specification substring 
                        var computerSpecifications = computerInformationMatch[computerSpecificationStartIndex..computerSpecificationEndIndex].Replace("&quot;", "\"").Replace("&#x27;", "'");

                        // Assign extracted substrings to the appropriate computer property and then add the computer object to the list of computers
                        computer = _extractSpecification.ExtractFromString(computerSpecifications);
                        computer.Type = computerType;
                        computer.Model = GetSubString(computerInformationMatch, computerModelStartIndex, "</span>", computerSpanHTMLLength);
                        computer.SKU = GetSubString(computerInformationMatch, computerSKUStartIndex, "</span>", computerSpanHTMLLength);
                        computer.Link = computerInformationMatch[linkStartIndex..linkEndIndex];
                        computer.Cost = computerPriceMatches[i].Groups[1].Value.Replace("<!-- -->", "");
                        CheckAvailabilityAndAddToList(computerInformationMatch, computerAvailabilityMatches[i].Groups[0].Value, computer);
                    }

                    return true;
                }
                else
                {
                    _errors.Add(new LogMessage
                    {
                        LogDate = DateTime.Now,
                        Method = "GetDesktopInformationFromPage",
                        Parameters = "Path string: " + pathString,
                        Message = $"Status code {(int)response.StatusCode}: {response.StatusCode}"
                    });
                }
            } 
            catch (Exception e)
            {
                _errors.Add(new LogMessage 
                {
                    LogDate = DateTime.Now,
                    Method = "GetDesktopInformationFromPage",
                    Parameters = "Path string: " + pathString,
                    Message = e.Message
                });                
            }
            return false;
        }

        // Method to get the substring while accounting for the starting index
        private string GetSubString(string computerSpecificationString, int startIndex, string endIndexString, int indexOffset)
        {
            if (startIndex != -1)
            {
                var endIndex = computerSpecificationString.IndexOf(endIndexString, startIndex);
                return computerSpecificationString.Substring(startIndex + indexOffset, endIndex - startIndex - indexOffset);
            }
            return "N/A";
        }

        // Method to check the 'add to cart' button to determine availability and condition of the computer and add to respective list
        private void CheckAvailabilityAndAddToList(string computerSpecificationString, string computerAvailabilityString, Computer computer)
        {
            if (computerAvailabilityString.ToLower().Contains("add to cart"))
            {
                if (computerSpecificationString.ToLower().Contains("refurbished")) {
                    computer.Availability = "Refurbished";
                    _refurbishedComputers.Add(computer);
                } else
                {
                    computer.Availability = "New";
                    _newComputers.Add(computer);
                }
            }
            else if (computerAvailabilityString.ToLower().Contains("check stores") || computerAvailabilityString.ToLower().Contains("unavailable nearby")) 
            {
                computer.Availability = "Check Stores";
                _unavailableComputers.Add(computer);
            }
            else if (computerAvailabilityString.ToLower().Contains("shop open-box"))
            {
                computer.Availability = "Open-Box";
                _openBoxComputers.Add(computer);
            } 
            else if (computerAvailabilityString.ToLower().Contains("sold out"))
            {
                computer.Availability = "Sold Out";
                _unavailableComputers.Add(computer);
            } 
            else if (computerAvailabilityString.ToLower().Contains("coming soon"))
            {
                computer.Availability = "Coming Soon";
                _unavailableComputers.Add(computer);
            } 
            else
            {
                computer.Availability = "N/A";
                _unavailableComputers.Add(computer);
            }
        }
    }

    
}
