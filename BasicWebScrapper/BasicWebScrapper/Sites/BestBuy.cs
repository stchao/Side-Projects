using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    class BestBuy : WebsiteComputers
    {
        private readonly HttpClient _httpClient;

        // private regex variables for extracting information 
        private readonly Regex _computerInformationRegex = new Regex("<div class=\"sku-title\">.*<\\/div>");
        private readonly Regex _computerPriceRegex = new Regex("<span(?: class=\"open-box-lowest-price\")? aria-hidden=\"true\">([$](?:<!-- -->)?\\d{0,2}?,?\\d{0,3}.\\d{0,2})<\\/span>");
        private readonly Regex _lastPageNumberRegex = new Regex("class=\"trans-button page-number\" aria-label=\"Results Page \\d\\d\">(\\d\\d)");
        private readonly Regex _computerAvailabilityRegex = new Regex("(?:style=\"padding:0 8px\">)(.*)(?:<\\/button>|<\\/a>)");

        // Initializing private variables
        public BestBuy(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BestBuy");
        }

        // Method to get the computer information from the page of the path string
        public override async Task<bool> GetInformationFromPage(string pathString, bool firstPage, string computerType)
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
                        computer = ExtractFromString(computerSpecifications);
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
    }

    
}
