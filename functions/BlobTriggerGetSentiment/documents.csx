#r "Newtonsoft.Json"

using Newtonsoft.Json;

private class TextRequest
{
    public TextRequest()
    {
        Documents = new List<TextDocument>();
    }

    [JsonProperty("documents")]
    public List<TextDocument> Documents { get; set; }
}
        
private class TextDocument
{
    public TextDocument(string id, string text, string language)
    {
        Id = id;
        Language = language;
        Text = text;
    }

    [JsonProperty("language")]
    public string Language { get; private set; }

    [JsonProperty("id")]
    public string Id { get; private set; }

    [JsonProperty("text")]
    public string Text { get; private set; }
} 