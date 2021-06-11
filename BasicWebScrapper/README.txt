Current Version 1.0.2

Change Log:
Version 1.0.2:
- Added methods to handle creating an Excel document for logs, or adding to the Excel document if there already is one
- Modified helper methods that check if a file exists or returns an available file name to take different file type extensions
- Average run time is (first: 0:23.2005635; second: 0:22.2797148)

Version 1.0.1:
- Added log classes to capture error log data and modified methods to capture any errors
- Added availability as another property of computers to show if the computer is available: new or refurbished, out of stock, open box, or in-store only
- Modified methods to allow sheets to be added and then separately exported, rather than creating only one sheet and then exporting it to Excel immediately
- Average run time is 0:22.8050803 (first: 0:22.5809934; second: 0:23.8027618; third: 0:22.0314856)

Version 1.0.0:
- Removed while loop with XPath expressions to get the last page number and the nodes/tags that contain the computer specification information
- Added IHttpClientFactory and utilized named clients and async methods to get the computer specification information from every single page of the desktop computer category and to handle getting the last page number
- Added Regex to extract the computer specification information indexes from the pages and the substring method to extract the strings 
- Run time of the async methods: O(2N or N) where N is the number of pages
- Average run time is 0:25.0142251 (first: 0:25.0142251)

Version 0.0.8:
- Modified the regex in loop method (#5) to include the common edge cases for the occasional typos in the anchor strings that contained the computer information. Added regexes to help extract uncommon edge cases and remove all forms of puncuations and whitespace characters. This reduced overall errors and the methods were able to capture most, if not, all of the information with very few errors.
- Removed dictionary and utilized for loop and string.contains to extract computer brand
- Updated class names, organized the methods to improved readibility, and added comments
- Average run time is 7:44.6867288 (first: 7:44.8881705; second: 7:41.5488792; third: 07:47.6231368)

Version 0.0.7:
- Removed previous for loop method (#4)
- Added an inner for loop method (#5) that is similar to the previous for loop method (#4) but regex is used instead to extract the information and get the indexes except the brand, which is obtained by seeing if the key exists in the dictionary in a constructed array formed by splitting the anchor string that contained the computer information by multiple strings of punctuation. This seemed to more accurately obtain the information with less instances of overlap, but still similar issues with overlap that could be resolved by removing punctuation. Computer model is still not included.
- Run time of the for loop: O(6N or N) where N is running regex expression against the string or looping through an array to get the brand
- Average run time is 7:50.9777583 (first: 7:54.1035223; second: 7:50.5481144; third: 7:48.2816382)

Version 0.0.6:
- Removed previous for loop method (#3)
- Added an inner for loop method (#4) that is a combination of the previous for loops but utilizing the indexes of where the first instance of a generic, common word for brands, cpu models, gpu models, storage sizes, and ram sizes was found from the anchor string that contained it. The idea was that since the information usually followed one after the other, these indexes could be compared with each other to extract the information. This seemed to more accurately obtained the information compared to the previous for loop methods (#1 to #3), but still similar issues as with the previous for loop (#3). Computer model is still not included.
- Run time of the for loop: O(N) where N is the size of the string array
- Average run time is 7:39.8961119 (first: 7:38.2893560, second: 7:41.5028679)

Version 0.0.5:
- Removed previous for loop methods (#1 and #2) and stopped use of dictionary for all other computer specifications except brand
- Added an inner for loop method (#3) that is a combination of the previous for loop methods (#1 and #2) but utilizing specific generic, common words for brands, cpu models, gpu models, storage sizes, and ram sizes to match elements of an array. The array was created using the anchor string that contained the information that was split by multiple strings of punctuation. This seemed to more accurately obtained the information compared to previous for loop methods (#1 and #2), but still similar issues as with the last two for loop methods (#1 and #2), in addition to having overlapping fields due to the generic words used for matching. Computer model is still not included.
- Run time of the for loop: O(N) where N is the size of the string array
- Added method to resolve file name conflicts when saving the Excel document
- Average run time is 07:46.9171944 (first: 7:39.8436998; second: 7:46.2590581; third: 7:54.6488255)

Version 0.0.4:
- Added another for loop within the previous for loop method (#2) to more accurately get the storage size from the anchor string that contained the computer information, potentially increasing run time to O(N^2)
- Added null coalescing operators to the method that exported to Excel
- Resolved issue where the storage sizes and title were not exporting to the Excel document
- Average run time is 7:21.6681872 (first: 7:21.6681872)

Version 0.0.3:
- Added dictionary to store all possible brands, cpu models, and ram sizes
- Added an inner for loop method (#2) that split the anchor string that contained the brand, cpu model, and ram size into an array by hypen surrounded by blank spaces(" - "), and utilized the dictionary to see if the elements in the array were keys in the dictionary and updated the property of the computer object based on the value returned from the dictionary. The biggest downside, similar to previous for loop method (#1) is if it doesn't match because of an extra space or punctuation, or doesn't split correctly, mutliple properties can get ignored or overlapped. GPU and computer models are not included in the dictionary.
- Run time of the for loop: O(2N or N) where N is the size of the string array or adding to dictionary
- Ran into issue where the storage sizes and title were not exporting to the Excel document
- Average run time is 07:43.6123150 (first: 07:43.6123150)

Version 0.0.2:
- Added method to export to Excel using the ClosedXML library
- Modified the link string to include the domain name before it was assigned to the link property of a computer object
- Added null conditional operators in the while loop when getting the model number, sku, cost, and link to account for computers without those information
- Resolved issue where it was only looping through the first page by updating URL for subsequent pages
- Average run time is 07:40.0204332 (first: 07:40.0204332; second: 7:32.1242980)

Version 0.0.1: 
- Added a while loop with XPath expressions to get the last page number and the nodes/tags that contain the computer specification information.
- Added an inner for loop method (#1) to utilize the contains method of the string class to loop through an array and see if any elements of the array were in the string and if so, return that element. There were arrays for brand, CPU, RAM, and storage; and they contained all possible specifications. The biggest downside is if it doesn't match because of an extra space or punctuation, it gets ignored. There are no arrays for GPU and computer models.
- Run time of the XPath expressions: O(5N or N) where N is the expressions
- Run time of the for loop: O(5N or N) where N is the size of the brand, CPU, RAM, and storage arrays
- Issue where the while loop is repeatedly looping the first page
- Average run time is 7:23.7669286 (first: 7:22.7933780; second: 7:24.6682119; third: 7:23.8391959)

Version 0.0.0: Basic web scrapper utilizing HtmlAgilityPack library and XPath expressions to get the computer information listed on the first page of BestBuy's desktop computers