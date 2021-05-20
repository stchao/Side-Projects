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
            string[] separatingStrings = { " - ",  " – ", "- ", ",", " -", "-,"};
            string[] computerSpecifications = specificationString.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
            
            if (computerSpecifications.Length > 3)
            {
                Computer computer = new Computer
                {
                    Title = specificationString,
                    Brand = CheckSpecification(computerSpecifications, "brand") ?? "N/A",
                    Storage = CheckSpecification(computerSpecifications, "storage") ?? "N/A",
                    CPU = CheckSpecification(computerSpecifications, "cpu") ?? "N/A",
                    RAM = CheckSpecification(computerSpecifications, "ram") ?? "N/A",
                    GPU = CheckSpecification(computerSpecifications, "gpu") ?? "N/A",
                };
                return computer;
            }

            return new Computer { Title = specificationString };
        }

        public string CheckSpecification(string[] specificationArray, string computerSpecificationType)
        {
            for (int i = 0; i < specificationArray.Length; i++)
            {
                var singleSpecificationString = specificationArray[i].ToLower();
                switch (computerSpecificationType)
                {
                    case "brand":
                        if (specificationDictionary.TryGetValue(singleSpecificationString.Trim().ToLower(), out _))
                        {
                            return specificationArray[i];
                        }
                        break;
                    case "cpu":
                        if ((singleSpecificationString.Contains("intel") || singleSpecificationString.Contains("apple m1")) || 
                            (singleSpecificationString.Contains("amd") && (singleSpecificationString.Contains("ryzen") || singleSpecificationString.Contains("a4"))))
                        {
                            return specificationArray[i];
                        }
                        break;
                    case "storage":
                        if ((singleSpecificationString.Contains("gb") || singleSpecificationString.Contains("tb")) && 
                            !singleSpecificationString.Contains("memory"))
                        {
                            return specificationArray[i];
                        }
                        break;
                    case "ram":
                        if ((singleSpecificationString.Contains("gb") || singleSpecificationString.Contains("tb")) && singleSpecificationString.Contains("memory"))
                        {
                            return specificationArray[i];
                        }
                        break;
                    case "gpu":
                        if ((singleSpecificationString.Contains("nvidia") || singleSpecificationString.Contains("amd")) && 
                            !(singleSpecificationString.Contains("ryzen") || singleSpecificationString.Contains("A4") ||singleSpecificationString.Contains("athlon")))
                        {
                            return specificationArray[i];
                        }
                        break;
                }                
            }
            return null;
        }
    }
}
