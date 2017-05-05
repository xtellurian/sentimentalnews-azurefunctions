#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public class SourceTableEntity : TableEntity 
{
    public string Id { get; set; }
    public string RunId {get;set;}
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string Category { get; set; }
    public string Language { get; set; }
    public string Country { get; set; }
}

public class ArticleTableEntity : TableEntity
{
    public string Id { get;set;}   
    public string RunId {get;set;} 
    public string Author { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string SourceId {get;set;}
    public string Url { get; set; }
    public string UrlToImage { get; set; }
    public DateTime? PublishedAt { get; set; }
    public double Sentiment {get;set;}
    public string Language {get;set;}
    public string TopicAssignments {get;set;}

    public void SetTopicAssignment (IEnumerable<TopicAssignment> assignments)
    {
        var serialised = "";
        if(assignments == null) return;
        foreach(var a in assignments)
        {
            serialised += $"{a.TopicId}:{a.Distance}:{a.TopicKeyPhrase},";
        }
        TopicAssignments = serialised;
    }

    public IEnumerable<TopicAssignment> GetTopicAssignments ()
    {
        var result = new List<TopicAssignment>();
        var split1 = TopicAssignments.Split(',');

        foreach(var s in split1)
        {
            var a = new TopicAssignment();
            var data = s.Split(':');
            a.TopicId = data[0];
            a.Distance = double.Parse(data[1]);
            if(data.Length > 2) a.TopicKeyPhrase = data[2]; 
            result.Add(a);
        }
        return result;
    }
}

public class TopicTableEntity : TableEntity 
{
    public string Id {get;set;}
    public double Score {get;set;}
    public string KeyPhrase {get;set;}
    public string RunId {get;set;}
}

public class TopicAssignment
{
    public string TopicId { get; set; }
    public double Distance { get; set; }
    public string TopicKeyPhrase {get;set;}
}

public class MetaEntity : TableEntity 
{
    public DateTime LastAccessed {get;set;}
    public string LatestRunId {get;set;}
}