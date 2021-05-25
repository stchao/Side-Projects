using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BasicWebScrapper
{    
    class ComputerSpecificationDictionary
    {
        private Dictionary<string, string> specificationDictionary = new Dictionary<string, string>();

        public void AddArrayToDictionary(string[] specificationArray, string computerSpecificationType)
        {
            foreach(string specification in specificationArray)
            {
                specificationDictionary.Add(specification.ToLower(), computerSpecificationType);
            }
        }

        public Computer CheckSpecifications(string specificationString)
        {
            string[] separatingStrings = { " - ", " – ", "- ", ",", " -", "-,", "-", " "};
            string[] currentComputerSpecification = specificationString.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
            var ramRegex = new Regex(@"(\d{1,3}\s?GB\s?(?:memory|ram)|\d{1,3}\s?GB(?!.*?Storage|[\w|.\w\s]*(?:SSD|HDD)|.?(?:Solid.*State|Hard).*Drive|.?EMMC))", RegexOptions.IgnoreCase);
            var storageRegex = new Regex(@"(\d{1,3} ?(?:G|T)B ?(?:NVMe.*|SATA.*)?(?:Flash.*Storage|SSD|HDD|(?:Solid.*State|Hard|Fusion).*Drive|.?EMMC))", RegexOptions.IgnoreCase);
            var cpuRegex = new Regex(@"(intel|apple m1|amd)(?! Radeon| Gaming|.*NUC)", RegexOptions.IgnoreCase);
            var gpuRegex = new Regex(@"(nvidia|amd)(?: RTX)?(?! Ryzen| Gaming)", RegexOptions.IgnoreCase);
            var ramMatches = ramRegex.Matches(specificationString);
            var storageMatches = storageRegex.Matches(specificationString);
            var cpuMatches = cpuRegex.Matches(specificationString);
            var gpuMatches = gpuRegex.Matches(specificationString);
            int[] computerSpecificationsIndexes;
            var computer = new Computer();

            for (var i = 0; i < currentComputerSpecification.Length; i++)
            {
                var currentComputerSpec = currentComputerSpecification[i].ToLower();
                if (specificationDictionary.TryGetValue(currentComputerSpec.Trim().ToLower(), out _))
                {
                    computer.Brand = currentComputerSpecification[i];
                    break;
                }              
            }

            int cpuIndex = cpuMatches.Count > 0 ? cpuMatches[0].Index : -1;
            int gpuIndex = gpuMatches.Count > 0 ? gpuMatches[0].Index : -1;
            int ramIndex = ramMatches.Count > 0 ? ramMatches[0].Index : -1;
            int storageIndex = storageMatches.Count > 0 ? storageMatches[0].Index : -1;
            int lastItemIndex = specificationString.IndexOf(currentComputerSpecification[^1]);
            computerSpecificationsIndexes = new int[] { cpuIndex, ramIndex, storageIndex, gpuIndex, lastItemIndex };

            string cpuString = cpuIndex > -1 ? GetSpecification(cpuIndex, GetNextNumber(cpuIndex, computerSpecificationsIndexes, specificationString.Length), specificationString) : "";
            string gpuString = gpuIndex > -1 ? GetSpecification(gpuIndex, GetNextNumber(gpuIndex, computerSpecificationsIndexes, specificationString.Length), specificationString) : "";
            string ramString = ramIndex > -1 ? GetMatchedStrings(ramMatches) : "";
            string storageString = storageIndex > -1 ? GetMatchedStrings(storageMatches) : "";
            computer.CPU = cpuString != "" ? cpuString : "N/A";
            computer.GPU = gpuString != "" ? gpuString : "N/A";
            computer.RAM = ramString != "" ? ramString : "N/A";
            computer.Storage = storageString != "" ? storageString : "N/A";

            return computer;
        }

        public int GetNextNumber(int currentNumber, int[] numbersArray, int stringLength)
        {
            var nextClosestNumber = -1;
            var difference = stringLength;
            for(int i = 0; i < numbersArray.Length; i++)
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

        public string GetSpecification(int startIndex, int endIndex, string specificationString)
        {
            if (endIndex != -1)
            {
                return specificationString.Substring(startIndex, endIndex - startIndex).Replace("-", " ").Trim();
            } else
            {
                return specificationString.Substring(startIndex, specificationString.Length - startIndex).Replace("-", " ").Trim();
            }
        }

        public string GetMatchedStrings(MatchCollection regexMatches)
        {
            var returnString = "";
            for (int i = 0; i < regexMatches.Count; i++)
            {
                returnString += regexMatches[i].Value + " ";
            }
            return returnString.Trim();
        }
    }
}
