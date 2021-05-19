using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            string[] separatingStrings = { " - " };
            string[] computerSpecifications = specificationString.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
            Computer computer = new Computer { Title = specificationString, Storage = CheckStorage(computerSpecifications) ?? "N/A" };

            foreach (string specification in computerSpecifications)
            {
                if (specificationDictionary.ContainsKey(specification.Trim().ToLower()))
                {
                    string value = "";
                    specificationDictionary.TryGetValue(specification.Trim().ToLower(), out value);
                    switch (value)
                    {
                        case "brand":
                            computer.Brand = specification.Trim();
                            break;
                        case "model":
                            computer.Model = specification.Trim();
                            break;
                        case "cpu":
                            computer.CPU = specification.Trim();
                            break;
                        case "ram":
                            computer.RAM = specification.Trim();
                            break;
                        default:
                            break;
                    }
                }
            }

            return computer;
        }

        public string CheckStorage(string[] specificationArray)
        {
            for (int i = 0; i < specificationArray.Length; i++)
            {
                var singleSpecificationString = specificationArray[i].ToLower();
                if ((singleSpecificationString.Contains("gb") || singleSpecificationString.Contains("tb")) && !singleSpecificationString.Contains("memory"))
                {
                    return singleSpecificationString;
                }
            }
            return null;
        }

    }
}
