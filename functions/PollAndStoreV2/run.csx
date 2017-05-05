#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#load "model.csx"
using System;
using System.Text;
using System.Net;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(CloudTable metadataTable, HttpRequestMessage req, TraceWriter log, Stream output)
{    
    if(CheckMetadata(metadataTable, log)){
        var headerValues = req.Headers.GetValues("x-location");
        var dataLocation = headerValues.FirstOrDefault();
        log.Info($"Adding serialised data to queue");

        var content = await req.Content.ReadAsStringAsync();
        var jObj = JObject.Parse(content);
        jObj.Add("dataLocation", new JValue(dataLocation));
        var result = JsonConvert.SerializeObject(jObj);
        var bytes = Encoding.UTF8.GetBytes(result);
        await output.WriteAsync(bytes, 0, bytes.Length);

        // return success message
        var response = new HttpResponseMessage();
        response.Content = new StringContent(JsonConvert.SerializeObject(
            new ResponseMessage(true)));
        return response;
    }
    else
    {
        // return error message
        var response = new HttpResponseMessage();
        response.Content = new StringContent(JsonConvert.SerializeObject(
            new ResponseMessage(false, "Called too soon")));
        return response;
    }
}

private static string PartitionKey = "AAA";
private static string RowKey = "001";
public static bool CheckMetadata(CloudTable table, TraceWriter log)
{
    TableOperation operation = 
        TableOperation.Retrieve<PollAndStoreMetadata>(PartitionKey, RowKey);
    TableResult result = table.Execute(operation);
    PollAndStoreMetadata meta;
    if(result.Result == null ){
        // this is the first time we're creating this meta field
        meta = new PollAndStoreMetadata();
        meta.PartitionKey = PartitionKey;
        meta.RowKey = RowKey;
        meta.LastAccessed = DateTime.Now;

        operation = TableOperation.Insert(meta);
        table.Execute(operation);
        log.Info($"Created first metadata with LastAccessed: {meta.LastAccessed}");
        return true;
    }
    else{
        meta = (PollAndStoreMetadata)result.Result;
    }
    

    log.Info($"Last Accessed at {meta.LastAccessed}");
    var diffInMinutes = (DateTime.Now - meta.LastAccessed).Value.TotalMinutes;
    if( (DateTime.Now - meta.LastAccessed) > new TimeSpan(1,0,0) ) // 1 hr
    {
        meta.LastAccessed = DateTime.Now;
        operation = TableOperation.Replace(meta);
        table.Execute(operation);
        log.Info($"Last Successful call was {diffInMinutes} mins ago");
        log.Info($"Updated metadata with time: {meta.LastAccessed}");
        return true;
    }
    else
    {
        log.Info($"Calling function too soon. Last called {diffInMinutes} mins ago");
        return false;
    }
}
