Current Version 0.0.0

Change Log:
Version 0.0.2:
- Added method to export to Excel using the ClosedXML library
- Modified computer link before assignment to a computer object so that it is a full link
- Added null conditional operators when getting the model number, sku, cost, and link to account for computers without those information
- Resolved issue where it was only looping through the first page repeatly by updating link
- Average run time is 07:40.0204332 (first: 07:40.0204332)

Version 0.0.1: 
- Added XPath expression to get last page number and a loop method to go through all the available pages of BestBuy's desktop computers
- Added for loop method, with a worst case run time of O(N), to parse the inner texts of anchor links that have computer information to extract brand, CPU, RAM, and storage. Parsing does not include GPU or Model.
- Average run time is 7:23.7669286 (first: 7:22.7933780; second: 7:24.6682119; third: 7:23.8391959)

Version 0.0.0: Basic web scrapper utilizing HtmlAgilityPack and XPath expressions to get the computer information listed on the first page of BestBuy's desktop computers

