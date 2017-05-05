#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#load "model.csx"

using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public static void Run(string myBlob, string runId, TraceWriter log,
CloudTable articlesTable, CloudTable topicsTable, CloudTable sourcesTable, CloudTable metaTable )
{
    log.Info($"Loaded data \n Size: {myBlob.Length} Bytes");
    var root = JObject.Parse(myBlob);
    var articles = new List<JToken>();
    foreach(var source in root["sources"])
    {
        var entity = ConvertToSourceTableEntity(source, runId);
        var result = sourcesTable.Execute(TableOperation.InsertOrMerge(entity));
        foreach(var article in source["articles"])
        {
            var prop = new JProperty("source", source["id"]);
            article.Last.AddAfterSelf(prop);
            articles.Add(article);
        }
    }
    log.Info($"Updated Sources table");
    log.Info($"Extracted a list of {articles.Count} articles");

// go through topics and get name for assignments
    var topicDictionary = new Dictionary<string,string>();
    foreach(var topic in root["operationProcessingResult"]["topics"])
    {
       var entity = ConvertToTopicEntity(topic, runId);
       var result = topicsTable.Execute(TableOperation.InsertOrMerge(entity));
       topicDictionary.Add(topic["id"].Value<string>(), topic["keyPhrase"].Value<string>());
    }
    log.Info("Added topics to table");

    foreach(var topicAssignment in root["operationProcessingResult"]["topicAssignments"])
    {
        var docId = topicAssignment["documentId"].Value<string>();
        var topicId = topicAssignment["topicId"].Value<string>();
        var article = articles.FirstOrDefault(a => a["Id"].Value<string>() == docId);
        topicAssignment["keyPhrase"] = topicDictionary[topicId];
        if(article["TopicAssignments"].Type == JTokenType.Null)
        {
            article["TopicAssignments"] = new JArray();
        } 
        ((JArray)article["TopicAssignments"]).Add(topicAssignment);
        // log.Info($"article should have added topic ass {article}");
    }
    log.Info("Processed Topic Assignments");

    var countWithoutTopics = 0;
    foreach(var article in articles)
    {
        if(article["TopicAssignments"].Type == JTokenType.Null)
        {
            var title = article["title"];
            countWithoutTopics++;
        }
        var entity = ConvertToArticleEntity(article, runId);
        var result = articlesTable.Execute(TableOperation.InsertOrMerge(entity));
    }
    log.Info($"Added {articles.Count} articles to table. {countWithoutTopics} had no topic");


    //meta data update
    var retrieveOperation = TableOperation.Retrieve<MetaEntity>("AAA", "001");

    var retrievedResult = metaTable.Execute(retrieveOperation);
    var meta = retrievedResult.Result as MetaEntity;
    meta.LatestRunId = runId;
    var metaResult = metaTable.Execute(TableOperation.InsertOrMerge(meta));
    log.Info($"Updated metadata with latest runId: {runId}");
}

private static SourceTableEntity ConvertToSourceTableEntity(JToken token, string runId)
{
    var partitionKey = "SourcesV1"; // static partition key for now
    var entity = new SourceTableEntity();
    entity.Id = token["id"].Value<string>();
    entity.RunId = runId;
    entity.Name = token["name"].Value<string>();
    entity.Description = token["description"].Value<string>();
    entity.Url = token["url"].Value<string>();
    entity.Language = token["language"].Value<string>();
    entity.Category = token["category"].Value<string>();
    entity.Country = token["country"].Value<string>();

    // set rowkey and partitionkey
    entity.PartitionKey = partitionKey;
    entity.RowKey = entity.Id;
    return entity;
}

private static DateTime MinAzureDateTime = new DateTime(1601, 1, 1);
private static ArticleTableEntity ConvertToArticleEntity (JToken token, string runId)
{
    var entity = new ArticleTableEntity();
    entity.Id = token["Id"].Value<string>();
    entity.RunId = runId;
    entity.Author = token["author"]?.Value<string>();
    entity.Title = token["title"]?.Value<string>();
    entity.Description = token["description"]?.Value<string>();
    entity.SourceId = token["source"].Value<string>();
    entity.Url = token["url"]?.Value<string>();
    entity.UrlToImage = token["urlToImage"]?.Value<string>();
    entity.PublishedAt = token["publishedAt"].Value<DateTime?>();
    if(entity.PublishedAt < MinAzureDateTime) entity.PublishedAt = null;
    entity.Sentiment = token["Sentiment"]?.Value<double>() ?? 0.5;
    entity.Language = token["Language"]?.Value<string>();
    var topicAsses = new List<TopicAssignment>();
    foreach(var ta in token["TopicAssignments"])
    {
        var obj = new TopicAssignment();
        obj.TopicId = ta["topicId"].Value<string>();
        obj.Distance = ta["distance"].Value<double>();
        obj.TopicKeyPhrase = ta["keyPhrase"].Value<string>();
        topicAsses.Add(obj);
    }
    entity.SetTopicAssignment(topicAsses);

    // set partitionkey and rowkey
    entity.PartitionKey = entity.RunId;
    entity.RowKey = entity.Id;
    return entity;
}

private static TopicTableEntity ConvertToTopicEntity (JToken token, string runId)
{
    var entity = new TopicTableEntity();
    entity.Id = token["id"].Value<string>();
    entity.Score = token["score"].Value<double>();
    entity.KeyPhrase = token["keyPhrase"].Value<string>();
    entity.RunId = runId;

    // set table props
    entity.PartitionKey = entity.RunId;
    entity.RowKey = entity.Id;
    return entity;
}
