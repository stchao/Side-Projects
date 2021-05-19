Current Version 0.0.0

Change Log:
Version 0.0.4:
- Added for loop method to more accurately get the storage from the anchor tag title
- Resolved issue where storage and anchor tag titles were not exporting to the Excel document
- Average run time is (first: 7:21.6681872)

Version 0.0.3:
- Added dictionary to store all possible brands, cpu, gpu, ram, and storage
- Added method, with a worst case run time of O(N^2), to parse anchor tag title to utilize dictionary
- Ran into issue where storage and anchor tag title are not in the exported Excel document
- Average run time is (first: 07:43.6123150)

Version 0.0.2:
- Added method to export to Excel using the ClosedXML library
- Modified computer link before assignment to a computer object so that it is a full link
- Added null conditional operators when getting the model number, sku, cost, and link to account for computers without those information
- Resolved issue where it was only looping through the first page repeatly by updating link
- Average run time is 07:40.0204332 (first: 07:40.0204332; second: 7:32.1242980)

Version 0.0.1: 
- Added XPath expression to get last page number and a loop method to go through all the available pages of BestBuy's desktop computers
- Added for loop method, with a worst case run time of O(N^2), to parse the inner texts of anchor links that have computer information to extract brand, CPU, RAM, and storage. Parsing does not include GPU or Model.
- Average run time is 7:23.7669286 (first: 7:22.7933780; second: 7:24.6682119; third: 7:23.8391959)

Version 0.0.0: Basic web scrapper utilizing HtmlAgilityPack library and XPath expressions to get the computer information listed on the first page of BestBuy's desktop computers

