#r "Newtonsoft.Json"
#load "documents.csx"

using System;
using System.Net;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public static async Task<string> Run(string myBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob \n Size: {myBlob.Length} Bytes");
    var key = ConfigurationManager.AppSettings["cognitive_api_key"];
    var root = JObject.Parse(myBlob);
    

    var articles = new List<JToken>();

    foreach(var source in root["sources"].Children<JObject>()){
        var children = source["articles"].Children<JToken>();
        articles.AddRange(children);
    }

    log.Info($"Found {articles.Count} articles as Jtoken");

    for (int i=0; i * 100 < articles.Count; i++){
        var count = Math.Min(100, articles.Count - i * 100);
        await SetSentimentsBy100(articles.GetRange(i*100,count), key, log);
    }
    
    return JsonConvert.SerializeObject(root);
}


private static string serviceEndpoint = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/";
private static string[] supportedLanguages = {"en","es","fr","pt"};

//needs to be run in batches of 100
public static async Task<IEnumerable<JToken>> SetSentimentsBy100(IEnumerable<JToken> articles, string apiKey, TraceWriter log)
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
    log.Info($"There are {articles.Count()} articles");
    var request = new TextRequest();
    foreach(var a in articles){
        if(supportedLanguages.Contains(a["Language"].Value<string>())) {
            if(!string.IsNullOrEmpty(a["description"].Value<string>())){
                    request.Documents.Add(new TextDocument(a["Id"].Value<string>(), 
                    a["description"].Value<string>(), 
                    a["Language"].Value<string>()));
            }

        }
    }
    log.Info($"Created Requests with {request.Documents.Count()} documents");
    var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
    var result = await httpClient.PostAsync($"{serviceEndpoint}sentiment", content).ConfigureAwait(false);

    var response = JObject.Parse(await result.Content.ReadAsStringAsync());
    var ff = response["documents"].Children().ToList();

    foreach(var f in ff){
        var id = f["id"];
        var sentiment = f["score"];

        articles.FirstOrDefault(a=>a["Id"].Value<string>() == id.ToString())["Sentiment"]
                    = sentiment.Value<double>();

    }
    return articles;
}

