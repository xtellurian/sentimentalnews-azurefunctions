#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#load "model.csx"
#load "entityModel.csx"
#load "conversions.csx"

using System.Net;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log,
    CloudTable metaTable, CloudTable sourcesTable, CloudTable articlesTable, CloudTable topicsTable)
{    
    log.Info("C# HTTP trigger function processed a request.");
    
    var aggregate = new ArticleDataAggregate(); // this is the object to be returned

    // get id for latest run
    var retrieveOperation = TableOperation.Retrieve<MetaEntity>("AAA", "001");
    var retrievedResult = metaTable.Execute(retrieveOperation);

    var runId = ((MetaEntity)retrievedResult.Result).LatestRunId;

    // get articles with that runId
    var articles = new List<Article>();
    var articleQuery = new TableQuery<ArticleTableEntity>().Where(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, runId));

    foreach (var entity in articlesTable.ExecuteQuery(articleQuery))
    {
        var a = ConvertToArticle(entity);
        articles.Add(a);
    }
    aggregate.Articles = articles;
    log.Info($"Found {articles.Count} articles with RunId: {runId}");

    // get sources
    var sources = new List<SourceBase>();
     var sourcesQuery = new TableQuery<SourceTableEntity>().Where(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "SourcesV1"));

    foreach (var entity in sourcesTable.ExecuteQuery(sourcesQuery))
    {
        var s = ConvertToSource(entity);
        sources.Add(s);
    }
    aggregate.Sources = sources;
    log.Info($"Found {sources.Count} sources with partitionkey: SourcesV1");

    // get topics
    var topics = new List<Topic>();
     var topicsQuery = new TableQuery<TopicTableEntity>().Where(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, runId));

    foreach (var entity in topicsTable.ExecuteQuery(topicsQuery))
    {
        var s = ConvertToTopic(entity);
        topics.Add(s);
    }
    aggregate.Topics = topics;
    log.Info($"Found {topics.Count} topics with partitionkey: {runId}");

    // get meta
    var meta = new MetaData();
    meta.DateCreated = ((MetaEntity)retrievedResult.Result).LastAccessed;//bit of a fudge
    aggregate.Meta = meta;
    log.Info($"Created metadata with DateCreated as {meta.DateCreated}");
    // Fetching the name from the path parameter in the request URL
    return req.CreateResponse(HttpStatusCode.OK, aggregate);
    
}