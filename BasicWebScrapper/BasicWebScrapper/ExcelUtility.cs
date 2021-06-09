using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace BasicWebScrapper
{
    class ExcelUtility
    {
        private HelperMethods _helperMethods = new HelperMethods();
        private XLWorkbook _xlComputersWorkbook = new XLWorkbook();
        private XLWorkbook _xlLogsWorkbook = new XLWorkbook();
        private List<LogMessage> _errors = new List<LogMessage>();

        public List<LogMessage> Errors { get { return _errors; } }

        public bool AddComputersToWorkbook(string worksheetName, string domainString, List<Computer> computers)
        {
            try
            {
                var worksheet = _xlComputersWorkbook.Worksheets.Add(worksheetName);

                // Adding column names
                worksheet.Cell(1, 1).Value = "Title";
                worksheet.Cell(1, 2).Value = "Availability";
                worksheet.Cell(1, 3).Value = "Brand";
                worksheet.Cell(1, 4).Value = "Model";
                worksheet.Cell(1, 5).Value = "SKU";
                worksheet.Cell(1, 6).Value = "CPU";
                worksheet.Cell(1, 7).Value = "GPU";
                worksheet.Cell(1, 8).Value = "RAM";
                worksheet.Cell(1, 9).Value = "Storage";
                worksheet.Cell(1, 10).Value = "Cost";
                worksheet.Cell(1, 11).Value = "Link";

                for (int i = 0; i < computers.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = computers[i].Title ?? "N/A";
                    worksheet.Cell(i + 2, 2).Value = computers[i].Availability ?? "N/A";
                    worksheet.Cell(i + 2, 3).Value = computers[i].Brand ?? "N/A";
                    worksheet.Cell(i + 2, 4).Value = computers[i].Model ?? "N/A";
                    worksheet.Cell(i + 2, 5).Value = computers[i].SKU ?? "N/A";
                    worksheet.Cell(i + 2, 6).Value = computers[i].CPU ?? "N/A";
                    worksheet.Cell(i + 2, 7).Value = computers[i].GPU ?? "N/A";
                    worksheet.Cell(i + 2, 8).Value = computers[i].RAM ?? "N/A";
                    worksheet.Cell(i + 2, 9).Value = computers[i].Storage ?? "N/A";
                    worksheet.Cell(i + 2, 10).Value = computers[i].Cost ?? "N/A";
                    worksheet.Cell(i + 2, 11).Value = (computers[i].Link == null ? "N/A" : domainString + computers[i].Link);
                }

                worksheet.Columns().AdjustToContents();
                return true;
            }
            catch (Exception e)
            {
                _errors.Add(new LogMessage
                {
                    LogDate = DateTime.Now,
                    Method = "AddComputersToWorkbook",
                    Parameters = $"Worksheet Name: {worksheetName} | Number of computers: {computers.Count}",
                    Message = e.Message
                });
            }
            return false;            
        }

        public bool AddLogsToWorkbook(string worksheetName, List<LogMessage> logs)
        {
            try
            {
                var worksheet = _xlLogsWorkbook.Worksheets.Add(worksheetName);

                // Adding column names
                worksheet.Cell(1, 1).Value = "LogDate";
                worksheet.Cell(1, 2).Value = "Method";
                worksheet.Cell(1, 3).Value = "Parameters";
                worksheet.Cell(1, 4).Value = "Message";

                for (int i = 0; i < logs.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = logs[i].LogDate;
                    worksheet.Cell(i + 2, 2).Value = logs[i].Method ?? "N/A";
                    worksheet.Cell(i + 2, 3).Value = logs[i].Parameters ?? "N/A";
                    worksheet.Cell(i + 2, 4).Value = logs[i].Message ?? "N/A";
                }

                worksheet.Columns().AdjustToContents();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now}: AddLogsToWorkbook - Worksheet Name: {worksheetName} | Number of logs: {logs.Count} - {e.Message}");
            }
            return false;
        }

        public void ExportWorkbooksToExcel()
        {
            _xlComputersWorkbook.SaveAs($"{Directory.GetCurrentDirectory()}\\{_helperMethods.CheckAndGetFileName("ComputerList")}");
            //_xlLogsWorkbook.SaveAs($"{Directory.GetCurrentDirectory()}\\{_helperMethods.CheckAndGetFileName("BasicWebScrapperLogs")}");
            _xlComputersWorkbook.Dispose();
            _xlLogsWorkbook.Dispose();
        }
    }
}
