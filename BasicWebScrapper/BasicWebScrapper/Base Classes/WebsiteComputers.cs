using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicWebScrapper
{
    abstract class WebsiteComputers
    {
        // Variable to keep track of the number of pages
        protected int _lastPageNumber = 1;
        protected readonly List<LogMessage> _errors = new List<LogMessage>();
        protected readonly List<Computer> _newComputers = new List<Computer>();
        protected readonly List<Computer> _openBoxComputers = new List<Computer>();
        protected readonly List<Computer> _refurbishedComputers = new List<Computer>();
        protected readonly List<Computer> _unavailableComputers = new List<Computer>();

        // private string array that contains all available brands
        private readonly string[] _availableComputerBrands = new string[] { "Acer", "Alienware", "Apple", "ASUS", "Azulle", "CanaKit", "CLX", "CORSAIR", "CyberPowerPC",
                "CybertronPC", "Dell", "HP", "HP OMEN", "iBUYPOWER", "Lenovo", "Microsoft", "MSI", "OptiPlex", "Raspberry Pi", "Shuttle",
                "Skytech Gaming", "Thermaltake", "Intel" };

        // private regex variables for extracting specifications
        private readonly Regex _ramRegex = new Regex(@"\d{1,3} ?GB(?! [ESAN]).*?(?:memory|ram)|\d{1,4}MHz.*?(?:memory|ram)|\d{1,3} ?(?:G|M)B?.(?:memory|ram|ddr4)", RegexOptions.IgnoreCase);
        private readonly Regex _ramEdgeCaseRegex = new Regex(@"\d{1,3} ?GB", RegexOptions.IgnoreCase);
        private readonly Regex _storageRegex = new Regex(@"\d{1,4} ?(?:G?|T)B? ?(?:SATA.*)?(?:Flash.*Storage|S{2,3}D|HDD?|(?:Solid.*State|Hard|Fusion).*Drive|.?EMMC)|\d{1,3} ?(?:G|T)B? ?(?:M.2|NVMe.*?|Gen4)(?:.*SSD|HDD|.NVME)?", RegexOptions.IgnoreCase);
        private readonly Regex _storageEdgeCaseRegex = new Regex(@"\d{1,3} ?TB[+]\d{1,3}GB|\d{1,3}GB.(\d{1,3}GB)", RegexOptions.IgnoreCase);
        private readonly Regex _cpuRegex = new Regex(@"(intel|apple m1|amd|(?:core)?.?i\d[^\d])(?! Radeon| Gaming|.*NUC|.HD)|xeon", RegexOptions.IgnoreCase);
        private readonly Regex _gpuRegex = new Regex(@"nvidia|amd(?! ?Ryzen(?:™)? | ?[ATWR]| ?Gaming| ?GX)|radeon|rx|geforce|(?:r|g)tx|(?:intel )?U*HD [^ATW]|intel iris", RegexOptions.IgnoreCase);
        private readonly Regex _specificationRegex = new Regex(@"\p{P}|\p{Zs}");
        private readonly Regex _specificationMultipleSpacesRegex = new Regex(@"\p{Zs}{2,}");

        public List<LogMessage> Errors { get { return _errors; } }
        public List<Computer> NewComputers { get { return _newComputers; } }
        public List<Computer> OpenBoxComputers { get { return _openBoxComputers; } }
        public List<Computer> RefurbishedComputers { get { return _refurbishedComputers; } }
        public List<Computer> UnavailableComputers { get { return _unavailableComputers; } }


        // Method to get all desktop computers
        public async Task<bool> GetComputers(string firstPagePath, string followingPagePaths, string computerType)
        {
            // Get computers from first page only to extract last page
            bool successfullyRetrievedFirstPage = await GetInformationFromPage(firstPagePath, true, computerType);

            if (successfullyRetrievedFirstPage)
            {
                // Declare and create a task for each page up to and including the last page
                List<Task<bool>> successfullyRetrieveOtherPagesTasks = new List<Task<bool>>();
                for (int i = 1; i < _lastPageNumber + 1; i++)
                {
                    successfullyRetrieveOtherPagesTasks.Add(GetInformationFromPage(followingPagePaths + i, false, computerType));
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


        // Method to get the substring while accounting for the starting index
        public string GetSubString(string computerSpecificationString, int startIndex, string endIndexString, int indexOffset)
        {
            if (startIndex != -1)
            {
                var endIndex = computerSpecificationString.IndexOf(endIndexString, startIndex);
                return computerSpecificationString.Substring(startIndex + indexOffset, endIndex - startIndex - indexOffset);
            }
            return "N/A";
        }

        // Method to check the 'add to cart' button to determine availability and condition of the computer and add to respective list
        public void CheckAvailabilityAndAddToList(string computerSpecificationString, string computerAvailabilityString, Computer computer)
        {
            if (computerAvailabilityString.ToLower().Contains("add to cart"))
            {
                if (computerSpecificationString.ToLower().Contains("refurbished"))
                {
                    computer.Availability = "Refurbished";
                    _refurbishedComputers.Add(computer);
                }
                else
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

        public abstract Task<bool> GetInformationFromPage(string pathString, bool firstPage, string computerType);        

        // Method to extract the computer specifications from a given string
        public Computer ExtractFromString(string specificationString)
        {
            // Replace all double spaces with single space, '&quot;' with ", and 'Mini' with '- Mini' (for some edge cases when extracting substrings)
            specificationString = _specificationMultipleSpacesRegex.Replace(specificationString.Replace("Mini", "- Mini"), " ");

            // Check if the standard regex is able to extract the respective specification string, otherwise use edge case regex to extract 
            var ramMatch = UpdateMatch(_ramRegex, _ramEdgeCaseRegex, specificationString);
            var storageMatches = UpdateMatches(_storageRegex, _storageEdgeCaseRegex, specificationString);

            // Match the string with the respective regex
            var cpuMatch = _cpuRegex.Match(specificationString);
            var gpuMatch = _gpuRegex.Match(specificationString);

            // Check the value and return -1 if the value is "" (can't be found)
            int cpuIndex = cpuMatch.Value != "" ? cpuMatch.Index : -1;
            int gpuIndex = gpuMatch.Value != "" ? gpuMatch.Index : -1;
            int ramIndex = ramMatch.Index;
            int storageIndexEdgeCase = storageMatches.Count > 0 && storageMatches[0].Groups[1].Value == "" ? 0 : 1;
            int storageIndex = storageMatches.Count > 0 ? storageMatches[0].Groups.Count < 2 ? storageMatches[0].Index : storageMatches[0].Groups[storageIndexEdgeCase].Index : -1;

            int[] computerSpecificationsIndexes = new int[] { cpuIndex, ramIndex, storageIndex, gpuIndex };

            // Extract string based on indexes
            string cpuString = cpuIndex > -1 ? GetSpecification(cpuIndex, GetNextNumber(cpuIndex, computerSpecificationsIndexes, specificationString), specificationString) : "";
            string gpuString = gpuIndex > -1 ? GetSpecification(gpuIndex, GetNextNumber(gpuIndex, computerSpecificationsIndexes, specificationString), specificationString) : "";

            string ramString = ramMatch.Value;

            // Extract string based on indexes; and if there are more than 2 groups, concat the strings with a '+' symbol otherwise just get value
            string storageString = storageIndex > -1 ? storageMatches[0].Groups.Count < 2 ? string.Join(" + ", from Match match in storageMatches select match.Value) : storageMatches[0].Groups[storageIndexEdgeCase].Value : "";

            // Replace any punctuation with a space and then remove any doublespaces before assigning to the return object property
            var computer = new Computer
            {
                Title = specificationString,
                CPU = cpuString != "" ? _specificationMultipleSpacesRegex.Replace(cpuString.Replace("-", " ").Replace("–", " "), " ").Trim() : "N/A",
                GPU = gpuString != "" ? _specificationMultipleSpacesRegex.Replace(_specificationRegex.Replace(gpuString, " "), " ").Trim() : "N/A",
                RAM = ramString != "" ? _specificationMultipleSpacesRegex.Replace(_specificationRegex.Replace(ramString, " "), " ").Trim() : "N/A",
                Storage = storageString != "" ? _specificationMultipleSpacesRegex.Replace(_specificationRegex.Replace(storageString, " "), " ").Trim() : "N/A"
            };

            for (var i = 0; i < _availableComputerBrands.Length; i++)
            {
                var currentComputerSpec = _availableComputerBrands[i].ToLower();
                if (specificationString.ToLower().Contains(currentComputerSpec))
                {
                    computer.Brand = _availableComputerBrands[i];
                    break;
                }
            }

            return computer;
        }

        // Method to get the index of the next closest number
        private int GetNextNumber(int currentNumber, int[] numbersArray, string specificationString)
        {
            var nextClosestNumber = GetNextPunctuationIndex(currentNumber, specificationString);
            var difference = nextClosestNumber > -1 ? nextClosestNumber - currentNumber : specificationString.Length;
            for (int i = 0; i < numbersArray.Length; i++)
            {
                var currentDifference = numbersArray[i] - currentNumber;
                if ((currentNumber < numbersArray[i]) && (currentDifference < difference))
                {
                    nextClosestNumber = numbersArray[i];
                    difference = nextClosestNumber - currentNumber;
                }
            }
            return nextClosestNumber;
        }

        // Method to get the index of the next closest punctuation in the order of priority "," > "(M" | "(L" > " - " > " – " > "–"
        private int GetNextPunctuationIndex(int currentNumber, string specificationString)
        {
            var commaIndex = specificationString.IndexOf(",", currentNumber);

            if (commaIndex > -1)
            {
                return commaIndex;
            }
            else
            {
                var parensM = specificationString.IndexOf("(M", currentNumber);
                var parensL = specificationString.IndexOf("(L", currentNumber);
                if (parensM > -1 || parensL > -1)
                {
                    return parensM > -1 ? parensM : parensL;
                }
                else
                {
                    var hypenIndex = specificationString.IndexOf(" - ", currentNumber);
                    if (hypenIndex > -1)
                    {
                        return hypenIndex;
                    }
                    else
                    {
                        var longHypenIndex = specificationString.IndexOf(" – ", currentNumber);
                        if (longHypenIndex > -1)
                        {
                            return longHypenIndex;
                        }
                        else
                        {
                            return specificationString.IndexOf("–", currentNumber);
                        }
                    }
                }
            }
        }

        // Method to extract the substring based on given indexes
        private string GetSpecification(int startIndex, int endIndex, string specificationString)
        {
            // if there is an index of the next closest number, return the substring between them 
            if (endIndex != -1)
            {
                return specificationString[startIndex..endIndex];
            }
            // otherwise, extract substring from start index to the end of the given string
            else
            {
                return specificationString[startIndex..];
            }
        }

        // Method to return the edge case regex match if the current regex returns no results
        private Match UpdateMatch(Regex currentRegex, Regex newRegex, string specificationString)
        {
            var currentMatch = currentRegex.Match(specificationString);
            if (currentMatch.Value == "")
            {
                return newRegex.Match(specificationString);
            }
            return currentMatch;
        }

        // Method to return the edge case regex matches if the current regex returns no results
        private MatchCollection UpdateMatches(Regex currentRegex, Regex newRegex, string specificationString)
        {
            var currentMatches = currentRegex.Matches(specificationString);
            if (currentMatches.Count == 0)
            {
                return newRegex.Matches(specificationString);
            }
            return currentMatches;
        }
    }
}
