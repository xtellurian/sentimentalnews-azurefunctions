#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

public class AnalyseImageRequest
{
    [JsonProperty("url")]
    public string Url {get;set;}
}

// this is a minimal class
public class ArticleTableEntity : TableEntity
{
    public string UrlToImage {get;set;}
}