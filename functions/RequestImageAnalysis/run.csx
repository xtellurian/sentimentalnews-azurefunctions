#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#load "model.csx"

using System.Net;
using System.Configuration;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string articleId,string cachedData, Stream outputBlob, CloudTable articlesTable, TraceWriter log)
{    
    if(!string.IsNullOrEmpty(cachedData))
    {
        log.Info($"Cache exists for id= {articleId}");
        var obj = JObject.Parse(cachedData);
     //   var converted = JsonConvert.DeserializeObject<string>(cachedData);
        return req.CreateResponse(HttpStatusCode.OK, obj);
    }

    var key = ConfigurationManager.AppSettings["Ocp-Apim-Subscription-Key"];
    log.Info($"Querying Article Table for id = {articleId}");

    var articleQuery = new TableQuery<ArticleTableEntity>().Where(
        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, articleId));

    var queryResult = articlesTable.ExecuteQuery(articleQuery);

    if(queryResult.Count() != 1){
        log.Warning($"Query result returned not 1 article but {queryResult.Count()}");
    }
    var article = queryResult.FirstOrDefault();
    if(article==null) return req.CreateResponse(HttpStatusCode.NotFound, $"{articleId} not found");
    
    var analysis = await RequestImageAnalysis(log, key, article.UrlToImage);
    await CacheData(analysis, outputBlob);
    log.Info($"Cached Data for {articleId}");

    // Fetching the name from the path parameter in the request URL
    return req.CreateResponse(HttpStatusCode.OK, analysis);
}

private static string requestUrl = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze";
private static string reqParams = "?visualFeatures=Description&details=Celebrities&language=en"; 
private static async Task<JObject> RequestImageAnalysis(TraceWriter log, string apiKey, string imageUrl)
{
    log.Info($"Image Url = {imageUrl}");
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

    var requestData = new AnalyseImageRequest(){
        Url = imageUrl
    };
    var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
    var result = await httpClient.PostAsync(requestUrl+reqParams, content);
    var stringResult = await result.Content.ReadAsStringAsync();
    log.Info(stringResult);
    var data = JObject.Parse(stringResult);

    return data;
}

private static async Task CacheData(JObject data, Stream output)
{
    var text = data.ToString(Formatting.None);
    var bytes = Encoding.UTF8.GetBytes(text);
    await output.WriteAsync(bytes, 0, bytes.Length);
}