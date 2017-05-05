#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<string> Run(string myBlob, string name, TraceWriter log)
{
    var jObj = JObject.Parse(myBlob);
    var dataLocation = (string) jObj.SelectToken("['dataLocation']");

    log.Info($"C# Queue trigger function processed: {dataLocation}");

    var key = ConfigurationManager.AppSettings["cognitive_api_key"];

    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

    var content = await GetContent(httpClient, dataLocation, log);
    log.Info($"Content string was {content.Length} long");
    var contentObj = JObject.Parse(content);
    //merge data
    var finalContent = MergeResponses(jObj, contentObj);

    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

    return finalContent;
}

private async static Task<string> GetContent(HttpClient httpClient, string location, TraceWriter log)
{
    var count = 0;
    string content = "";
    while(true)
    {
        var topics = await httpClient.GetAsync(location);
        content = await topics.Content.ReadAsStringAsync();

        var response = JsonConvert.DeserializeObject<Response>(content);
        log.Info("Response: " + response.status);
        if(string.Equals(response.status, "Succeeded")) break;
        if(++count > 50) throw new Exception("Exceeeded Retry Count");
        await Task.Delay(30000); // wait 30 seconds
    }
    
    return content;
}

private static string MergeResponses(JObject o1, JObject o2)
{
    var mergeSettings = new JsonMergeSettings
    {
        MergeArrayHandling = MergeArrayHandling.Union
    };

    o1.Merge(o2, mergeSettings);
    return JsonConvert.SerializeObject(o1);
}

public class Response 
{
    public string status { get; set; }
}

    