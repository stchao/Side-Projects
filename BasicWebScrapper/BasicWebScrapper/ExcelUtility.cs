using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace BasicWebScrapper
{
    class ExcelUtility : HelperMethods
    {
        private readonly XLWorkbook _xlComputersWorkbook = new XLWorkbook();
        private XLWorkbook _xlLogsWorkbook = new XLWorkbook();
        private readonly List<LogMessage> _errors = new List<LogMessage>();

        private readonly string _logFileName = "BasicWebScrapperLogs";
        private readonly string _excelFileTypeExtension = ".xlsx";

        public List<LogMessage> Errors { get { return _errors; } }

        public bool AddComputersToWorkbook(string worksheetName, string domainString, List<Computer> computers)
        {
            try
            {
                var worksheet = _xlComputersWorkbook.Worksheets.Add(worksheetName);
                var offset = worksheetName != "Unavailable" ? 0 : 1;

                // Adding column names and only add the availability column if the worksheet name is unavailable, which
                // contains multiple conditions (sold out, check stores, unavailable, n/a, etc)
                if (worksheetName == "Unavailable") { worksheet.Cell(1, offset).Value = "Availability"; }                
                worksheet.Cell(1, 1 + offset).Value = "Brand";
                worksheet.Cell(1, 2 + offset).Value = "Model";
                worksheet.Cell(1, 3 + offset).Value = "Type";
                worksheet.Cell(1, 4 + offset).Value = "Cost";
                worksheet.Cell(1, 5 + offset).Value = "CPU";
                worksheet.Cell(1, 6 + offset).Value = "GPU";
                worksheet.Cell(1, 7 + offset).Value = "RAM";
                worksheet.Cell(1, 8 + offset).Value = "Storage";
                worksheet.Cell(1, 9 + offset).Value = "Link";
                worksheet.Cell(1, 10 + offset).Value = "SKU";
                worksheet.Cell(1, 11 + offset).Value = "Title";   

                for (int i = 0; i < computers.Count; i++)
                {
                    if (worksheetName == "Unavailable") { worksheet.Cell(i + 2, offset).Value = computers[i].Availability ?? "N/A"; }
                    worksheet.Cell(i + 2, 1 + offset).Value = computers[i].Brand ?? "N/A";
                    worksheet.Cell(i + 2, 2 + offset).Value = computers[i].Model ?? "N/A";
                    worksheet.Cell(i + 2, 3 + offset).Value = computers[i].Type ?? "N/A";
                    worksheet.Cell(i + 2, 4 + offset).Value = computers[i].Cost ?? "N/A";
                    worksheet.Cell(i + 2, 5 + offset).Value = computers[i].CPU ?? "N/A";
                    worksheet.Cell(i + 2, 6 + offset).Value = computers[i].GPU ?? "N/A";
                    worksheet.Cell(i + 2, 7 + offset).Value = computers[i].RAM ?? "N/A";
                    worksheet.Cell(i + 2, 8 + offset).Value = computers[i].Storage ?? "N/A";
                    worksheet.Cell(i + 2, 9 + offset).Value = (computers[i].Link == null ? "N/A" : domainString + computers[i].Link);
                    worksheet.Cell(i + 2, 10 + offset).Value = computers[i].SKU ?? "N/A";
                    worksheet.Cell(i + 2, 11 + offset).Value = computers[i].Title ?? "N/A";                   
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
            var logFileName = GetAvailableFileName(_logFileName, _excelFileTypeExtension);

            try
            {
                // If the file already exists
                if (logFileName == "BasicWebScrapperLogs.xlsx")
                {
                    // Get the path and assign the global workbook variable to the file found
                    var filePath = CreateCurrentDirectoryPath(_logFileName);
                    _xlLogsWorkbook = new XLWorkbook(filePath);

                    // Open the worksheet based on the given parameter and find the next empty row
                    var logsWorksheet = _xlLogsWorkbook.Worksheet(worksheetName);
                    var emptyRowNumber = GetEmptyRowNumber(logsWorksheet);

                    // If the workbook name is "Logs", use the AddLogs method. Otherwise, use the AddErrors method
                    if (worksheetName == "Logs")
                    {
                        // Adding log information rows
                        logsWorksheet = AddLogs(logsWorksheet, logs, emptyRowNumber);                        
                        return true;
                    }
                    else
                    {
                        // Adding error information rows
                        logsWorksheet = AddErrorLogs(logsWorksheet, logs, emptyRowNumber);                        
                        return true;
                    }
                }
                // If the file doesn't exist
                else
                {
                    // Add new worksheets with the desired name
                    var logsWorksheet = _xlLogsWorkbook.Worksheets.Add(worksheetName);

                    if (worksheetName == "Logs")
                    {
                        // Adding column names
                        logsWorksheet.Cell(1, 1).Value = "LogNumber";
                        logsWorksheet.Cell(1, 2).Value = "LogDate";
                        logsWorksheet.Cell(1, 3).Value = "Message";

                        // Adding log information rows
                        logsWorksheet = AddLogs(logsWorksheet, logs, 2);
                    } 
                    else
                    {
                        // Adding column names
                        logsWorksheet.Cell(1, 1).Value = "LogNumber";
                        logsWorksheet.Cell(1, 2).Value = "LogDate";
                        logsWorksheet.Cell(1, 3).Value = "Method";
                        logsWorksheet.Cell(1, 4).Value = "Parameters";
                        logsWorksheet.Cell(1, 5).Value = "Message";

                        // Adding error information rows
                        logsWorksheet = AddErrorLogs(logsWorksheet, logs, 2);
                    }                  

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now}: AddLogsToWorkbook - Worksheet Name: {worksheetName} | Number of logs: {logs.Count} - {e.Message}");
            }
            return false;
        }

        public void ExportWorkbooksToExcel()
        {
            _xlComputersWorkbook.SaveAs($"{Directory.GetCurrentDirectory()}\\{GetAvailableFileName("ComputerList", _excelFileTypeExtension)}");
            _xlLogsWorkbook.SaveAs($"{Directory.GetCurrentDirectory()}\\{_logFileName}{_excelFileTypeExtension}");
            _xlComputersWorkbook.Dispose();
            _xlLogsWorkbook.Dispose();
        }

        private int GetEmptyRowNumber(IXLWorksheet xLWorksheet)
        {
            int i = 1;
            while (!xLWorksheet.Row(i).IsEmpty())
            {
                i++;
            }
            return i;
        }

        private IXLWorksheet AddLogs(IXLWorksheet logsWorksheet, List<LogMessage> logs, int emptyRowNumber)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                logsWorksheet.Cell(i + emptyRowNumber, 1).Value = i + emptyRowNumber - 1;
                logsWorksheet.Cell(i + emptyRowNumber, 2).Value = logs[i].LogDate;
                logsWorksheet.Cell(i + emptyRowNumber, 3).Value = logs[i].Message ?? "N/A";
            }

            logsWorksheet.Columns().AdjustToContents();
            return logsWorksheet;
        }

        private IXLWorksheet AddErrorLogs(IXLWorksheet errorLogsWorksheet, List<LogMessage> logs, int emptyRowNumber)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                errorLogsWorksheet.Cell(i + emptyRowNumber, 1).Value = i + emptyRowNumber - 1;
                errorLogsWorksheet.Cell(i + emptyRowNumber, 2).Value = logs[i].LogDate;
                errorLogsWorksheet.Cell(i + emptyRowNumber, 3).Value = logs[i].Method ?? "N/A";
                errorLogsWorksheet.Cell(i + emptyRowNumber, 4).Value = logs[i].Parameters ?? "N/A";
                errorLogsWorksheet.Cell(i + emptyRowNumber, 5).Value = logs[i].Message ?? "N/A";
            }

            errorLogsWorksheet.Columns().AdjustToContents();
            return errorLogsWorksheet;
        }
    }
}
