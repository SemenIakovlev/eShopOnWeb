using Microsoft.Azure.Functions.Worker;

namespace AzureFunctions;

public class OrderResponse
{
    [BlobOutput("orders/{OrderId}.json", Connection = "AzureWebJobsStorage")]
    public string OrderJson { get; set; }
}

