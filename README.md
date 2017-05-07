# sentimentalnews-azurefunctions

These functions are currently being used by [Sentimental News](http://sentimental-news.azurewebsites.net/)

These functions are used to recieve data from Sentimental News, and process it though a chain of data-processing functions.

## Process:

0) Upload a corpus of News Articles to Azure Cognitive Services API. Currently this feature is not implemented in Azure Functions due to the lack of support for dotnet core - it should be done soon.

1) PollAndStoreDataV2 accepts a HTTP-Post containing all the articles recently pushed to the Azure Congitive Services API. This function simply places the data into blob storage and returns 200 to the client.

2) ProcessBlob is triggered by PollAndStoreDataV2 saving to blob store. It loads the data, and then polls/ waits for MS Azure Cognitive Services to provide the topic detection results for the corpus uploaded in step 0. Once the data are available, it is saved once again to blob storage.

3) BlobTriggerGetSentiment is triggered by ProcessBlob saving to blob store. This function processes every article for sentiment using the description property. Once sentiment is calculated, it is stored with every article and placed back into blob storage.

4) StoreDataInTables is triggered by BlobTriggerGetSentiment saving to blob store. This function loads the data from blob store and saves it to table store in a much more useful format. The rand-guid name for blobs becomes the runId.

5) GetLatestDataV2 accesses table store and builds a data structure to be returned to the client.

6) RequestImageAnalysis used Cognitive Services to analyse an image from an article. The data is saved to blob storage. On request, the cache is checked before returning either the cached data, or analysing the image, caching that response, and returning the data to the client.



## ToDo

- [ ] Implement step 0 in Azure Function
