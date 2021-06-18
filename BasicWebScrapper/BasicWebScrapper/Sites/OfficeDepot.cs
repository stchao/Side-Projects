using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicWebScrapper.Sites
{
    class OfficeDepot : WebsiteComputers
    {

        //"<div class=\\"sku_description_wrapper\\">"

        private readonly HttpClient _httpClient;

        // private regex variables for extracting information 
        private readonly Regex _specificationMultipleSpacesRegex = new Regex(@"\p{Zs}{2,}");

        private readonly Regex _computerInformationRegex = new Regex("<div class=\"desc_text \">.*?<\\/div>", RegexOptions.IgnoreCase);
        private readonly Regex _computerPriceRegex = new Regex("<span class=\"price_column right \">.(\\$\\d{1,5}.\\d{2,}).<\\/span>", RegexOptions.IgnoreCase);
        private readonly Regex _lastPageNumberRegex = new Regex("\"skuList_results_v2\">(\\d*).*?<\\/span>", RegexOptions.IgnoreCase);
        private readonly Regex _computerAvailabilityRegex = new Regex("<input type=\"submit\" value=\"Add to Cart\".*?title=\"Add to Cart\" \\/>", RegexOptions.IgnoreCase);

        // Initializing private variables
        public OfficeDepot(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("OfficeDepot");
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

                    bestBuyHTML = _specificationMultipleSpacesRegex.Replace(bestBuyHTML.Replace("\t", "").Replace("\n", "").Replace("\r", ""), " ");

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
                        var linkEndIndex = computerInformationMatch.IndexOf("\"", linkStartIndex);
                        var computerSpecificationStartIndex = linkEndIndex + "\" title=\"".Length;
                        var computerSpecificationEndIndex = computerInformationMatch.IndexOf("\"", computerSpecificationStartIndex);

                        // Variables to store the SKU and model indexes
                        var computerSpanHTMLLength = "product-id=\"".Length;
                        var computerSKUStartIndex = computerInformationMatch.IndexOf("product-id=\"");

                         // Variable to store the extracted computer specification substring 
                         var computerSpecifications = computerInformationMatch[computerSpecificationStartIndex..computerSpecificationEndIndex].Replace("&quot;", "\"").Replace("&#x27;", "'");

                        // Assign extracted substrings to the appropriate computer property and then add the computer object to the list of computers
                        computer = ExtractFromString(computerSpecifications);
                        computer.Type = computerType;
                        computer.Model = "N/A";
                        computer.SKU = GetSubString(computerInformationMatch, computerSKUStartIndex + computerSpanHTMLLength, "\"", 0);
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
