using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BasicWebScrapper
{
    class ExtractSpecification
    {
        // private string array that contains all available brands
        private readonly string[] _availableComputerBrands = new string[] { "Acer", "Alienware", "Apple", "ASUS", "Azulle", "CanaKit", "CLX", "CORSAIR", "CyberPowerPC",
                "CybertronPC", "Dell", "HP", "HP OMEN", "iBUYPOWER", "Intel", "Lenovo", "Microsoft", "MSI", "OptiPlex", "Raspberry Pi", "Shuttle",
                "Skytech Gaming", "Thermaltake" };

        // private regex variables for extracting specifications
        private readonly Regex _ramRegex = new Regex(@"\d{1,3} ?GB(?! [ESAN]).*?(?:memory|ram)|\d{1,4}MHz.*?(?:memory|ram)|\d{1,3} ?(?:G|M)B?.(?:memory|ram|ddr4)", RegexOptions.IgnoreCase);
        private readonly Regex _ramEdgeCaseRegex = new Regex(@"\d{1,3} ?GB", RegexOptions.IgnoreCase);
        private readonly Regex _storageRegex = new Regex(@"\d{1,4} ?(?:G?|T)B? ?(?:SATA.*)?(?:Flash.*Storage|S{2,3}D|HDD?|(?:Solid.*State|Hard|Fusion).*Drive|.?EMMC)|\d{1,3} ?(?:G|T)B? ?(?:M.2|NVMe.*?|Gen4)(?:.*SSD|HDD|.NVME)?", RegexOptions.IgnoreCase);
        private readonly Regex _storageEdgeCaseRegex = new Regex(@"\d{1,3} ?TB[+]\d{1,3}GB|\d{1,3}GB.(\d{1,3}GB)", RegexOptions.IgnoreCase);
        private readonly Regex _cpuRegex = new Regex(@"(intel|apple m1|amd|(?:core)?.?i\d[^\d])(?! Radeon| Gaming|.*NUC|.HD)|xeon", RegexOptions.IgnoreCase);
        private readonly Regex _gpuRegex = new Regex(@"nvidia|amd(?! ?Ryzen(?:™)? | ?[ATW]| ?Gaming| ?GX)|rx|geforce|(?:r|g)tx|(?:intel )?U*HD [^ATW]", RegexOptions.IgnoreCase);
        private readonly Regex _specificationRegex = new Regex(@"\p{P}|\p{Zs}");
        private readonly Regex _specificationMultipleSpacesRegex = new Regex(@"\p{Zs}{2,}");

        // Method to extract the computer specifications from a given string
        public Computer ExtractFromString(string specificationString)
        {
            // Replace all double spaces with single space, '&quot;' with ", and 'Mini' with '- Mini' (for some edge cases when extracting substrings)
            specificationString = _specificationMultipleSpacesRegex.Replace(specificationString.Replace("&quot;", "\"").Replace("Mini", "- Mini"), " ");

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
            var computer = new Computer {
                Title = specificationString,
                CPU = cpuString != "" ? _specificationMultipleSpacesRegex.Replace(cpuString.Replace("-", " ").Replace("–", " "), " ").Trim() : "N/A",
                GPU = gpuString != "" ? _specificationMultipleSpacesRegex.Replace(_specificationRegex.Replace(gpuString, " "), " ").Trim() : "N/A",
                RAM = ramString != "" ? _specificationMultipleSpacesRegex.Replace(_specificationRegex.Replace(ramString, " "), " ").Trim() : "N/A",
                Storage = storageString != "" ? _specificationMultipleSpacesRegex.Replace(_specificationRegex.Replace(storageString, " "), " ").Trim() : "N/A"
            };

            for (var i = 0; i < _availableComputerBrands.Length; i++)
            {
                var currentComputerSpec = _availableComputerBrands[i].ToLower();
                if (specificationString.Contains(currentComputerSpec))
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
