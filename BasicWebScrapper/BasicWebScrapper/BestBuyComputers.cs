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
        }
        public List<LogMessage> Errors { get { return _errors; } }

        // Method to get all desktop computers
        public async Task<List<Computer>> GetDesktopComputers(string firstPagePath, string followPagePaths)
        {
            // Get computers from first page only to extract last page
            List<Computer> computers = await GetInformationFromPage(firstPagePath, true);

            // Declare and create a task for each page up to and including the last page
            List<Task<List<Computer>>> computersTask = new List<Task<List<Computer>>>();
            for (int i = 1; i < _lastPageNumber + 1; i++)
            {
                computersTask.Add(GetInformationFromPage(followPagePaths + i, false));
            }

            // Wait until all the tasks are complete then merge lists of computers together
            var computersArray = await Task.WhenAll(computersTask);
            for (int j = 0; j < computersArray.Length; j++)
            {
                computers.AddRange(computersArray[j]);
            }

            return computers;
        }

        // Method to get the computer information from the page of the path string
        public async Task<List<Computer>> GetInformationFromPage(string pathString, bool firstPage)
        {
            try
            {
                var response = await _httpClient.GetAsync(pathString);

                // Continue if the 'GET' was successful, indicated by a status code of 'OK'
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var computers = new List<Computer>();
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
                        computer.Availability = CheckStringForAvailability(computerInformationMatch, computerAvailabilityMatches[i].Groups[0].Value);
                        computer.Model = GetSubString(computerInformationMatch, computerModelStartIndex, "</span>", computerSpanHTMLLength);
                        computer.SKU = GetSubString(computerInformationMatch, computerSKUStartIndex, "</span>", computerSpanHTMLLength);
                        computer.Link = computerInformationMatch[linkStartIndex..linkEndIndex];
                        computer.Cost = computerPriceMatches[i].Groups[1].Value.Replace("<!-- -->", "");
                        computers.Add(computer);
                    }

                    return computers;
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

            return new List<Computer>();
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

        // Method to check the 'add to cart' button to determine availability and condition of the computer
        private string CheckStringForAvailability(string computerSpecificationString, string computerAvailabilityString)
        {

            if (computerAvailabilityString.ToLower().Contains("add to cart"))
            {
                if (computerSpecificationString.ToLower().Contains("refurbished")) {
                    return "Yes: Refurbished";
                }                 
                return "Yes: New";
            }
            else if (computerAvailabilityString.ToLower().Contains("check stores") || computerAvailabilityString.ToLower().Contains("unavailable nearby")) 
            {
                return "Check Stores";
            }
            else if (computerAvailabilityString.ToLower().Contains("shop open-box"))
            {
                return "Yes: Open-Box";
            } else if (computerAvailabilityString.ToLower().Contains("sold out"))
            {
                return "No: Sold Out";
            } else if (computerAvailabilityString.ToLower().Contains("coming soon"))
            {
                return "No: Coming Soon";
            }
            return "N/A";
        }
    }

    
}
