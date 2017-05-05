#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"


using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

public class ArticleDataAggregate
{
    public MetaData Meta {get;set;}
    public List<Article> Articles {get; set;}
    public List<SourceBase> Sources {get; set;}
    public List<Topic> Topics {get; set;}
}

public class MetaData 
{
    public string DataLocation {get;set;}
    public DateTime? DateCreated {get;set;}
}

public class Article 
{
     public string Id { get;set;}
        
    [JsonProperty("author")]
    public string Author { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("urlToImage")]
    public string UrlToImage { get; set; }
    [JsonProperty("publishedAt")]
    public DateTime? PublishedAt { get; set; }
    public List<string> KeyPhrases {get;set;} // currently not using this
    public double Sentiment {get;set;}
    public string Language {get;set;}
    public List<TopicAssignment> TopicAssignments {get;set;}
}

public class SourceBase
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string Category { get; set; }
    public string Language { get; set; }
    public string Country { get; set; }
   //  public UrlsToLogos urlsToLogos { get; set; }
}

public class Topic 
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("score")]
    public double Score { get; set; }
    [JsonProperty("keyPhrase")]
    public string KeyPhrase { get; set; }

    public double AverageSentiment {get;set;}
}

public class TopicAssignment
{
    [JsonProperty("topicId")]
    public string TopicId { get; set; }
    [JsonProperty("documentId")]
    public string DocumentId { get; set; }
    [JsonProperty("distance")]
    public double Distance { get; set; }
    [JsonProperty("topicKeyPhrase")]
    public string TopicKeyPhrase {get;set;}
}