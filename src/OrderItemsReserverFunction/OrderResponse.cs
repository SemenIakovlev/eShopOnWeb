using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace OrderItemsReserverFunction;

public class OrderResponse
{
    [BlobOutput("https://eshop.blob.core.windows.net/orders/{Id}.json", Connection = "AzureWebJobsStorage")]
    public string OrderJson { get; set; }

    public HttpResponseData HttpResponse { get; set; }

    public int Id { get; set; }
}   

