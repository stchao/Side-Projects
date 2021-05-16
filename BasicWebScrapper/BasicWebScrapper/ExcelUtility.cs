using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace BasicWebScrapper
{
    class ExcelUtility
    {
        public void exportToExcel(List<Computer> computers)
        {
            using (var workbook = new XLWorkbook())
            {
                // Creating worksheet
                var worksheet = workbook.Worksheets.Add("BestBuy");

                // Adding column names
                worksheet.Cell(1, 1).Value = "Title";
                worksheet.Cell(1, 2).Value = "Brand";
                worksheet.Cell(1, 3).Value = "Model";
                worksheet.Cell(1, 4).Value = "Model Number";
                worksheet.Cell(1, 5).Value = "SKU";
                worksheet.Cell(1, 6).Value = "CPU";
                worksheet.Cell(1, 7).Value = "GPU";
                worksheet.Cell(1, 8).Value = "RAM";
                worksheet.Cell(1, 9).Value = "Storage";
                worksheet.Cell(1, 10).Value = "Cost";
                worksheet.Cell(1, 11).Value = "Link";

                for (int i = 0; i < computers.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = computers[i].Title;
                    worksheet.Cell(i + 2, 2).Value = computers[i].Brand;
                    worksheet.Cell(i + 2, 3).Value = computers[i].Model;
                    worksheet.Cell(i + 2, 4).Value = computers[i].ModelNumber;
                    worksheet.Cell(i + 2, 5).Value = computers[i].SKU;
                    worksheet.Cell(i + 2, 6).Value = computers[i].CPU;
                    worksheet.Cell(i + 2, 7).Value = computers[i].GPU;
                    worksheet.Cell(i + 2, 8).Value = computers[i].RAM;
                    worksheet.Cell(i + 2, 9).Value = computers[i].Storage;
                    worksheet.Cell(i + 2, 10).Value = computers[i].Cost;
                    worksheet.Cell(i + 2, 11).Value = (computers[i].Link == null ? "N/A" : "https://www.bestbuy.com/" + computers[i].Link);
                }

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(Directory.GetCurrentDirectory()+"\\ComputerList.xlsx");
            }            
        }
    }
}
