#load "model.csx"
#load "entityModel.csx"

public static Article ConvertToArticle(ArticleTableEntity entity)
{
    var article = new Article();
    article.Id = entity.Id;
    article.Author = entity.Author;
    article.Title = entity.Title;
    article.Description = entity.Description;
    article.Url = entity.Url;
    article.UrlToImage = entity.UrlToImage;
    article.PublishedAt = entity.PublishedAt;
    article.Sentiment = entity.Sentiment;
    article.Language = entity.Language;

    var assignments = new List<TopicAssignment>();
    foreach(var a in entity.GetTopicAssignments()){
        var x = new TopicAssignment();
        x.TopicId = a.TopicId;
        x.Distance = a.Distance;
        x.TopicKeyPhrase = a.TopicKeyPhrase;
        assignments.Add(x);
    } 

    article.TopicAssignments = assignments;
    return article;
} 

public static SourceBase ConvertToSource(SourceTableEntity entity)
{
    var source = new SourceBase();
    source.Id = entity.Id;
    source.Name = entity.Name;
    source.Description = entity.Description;
    source.Url = entity.Url;
    source.Category = entity.Category;
    source.Language = entity.Language;
    source.Country = entity.Country;

    return source;
}

public static Topic ConvertToTopic(TopicTableEntity entity)
{
    var topic = new Topic();
    topic.Id = entity.Id;
    topic.KeyPhrase = entity.KeyPhrase;
    topic.Score = entity.Score;
    // topic.AverageSentiment = entity.AverageSentiment;
    return topic;
}