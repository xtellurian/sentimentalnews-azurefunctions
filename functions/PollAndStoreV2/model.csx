#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Table;

public class ResponseMessage 
{
    public ResponseMessage (bool success, string message = "")
    {
        Success = success;
        Message = message;
    }
    public bool Success {get;set;}
    public string Message {get;set;}
}


public class PollAndStoreMetadata : TableEntity
{
    public DateTime? LastAccessed { get; set; }
}