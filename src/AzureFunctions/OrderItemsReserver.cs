using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;

namespace AzureFunctions;

public static class OrderItemsReserver
{
    [Function("OrderItemsReserver")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync() ?? String.Empty;
        var order = JsonConvert.DeserializeObject<OrderDto>(body);
        var response = req.CreateResponse();
        var result = new OrderResponse { OrderJson = JsonConvert.SerializeObject(order) };
        await response.WriteStringAsync(JsonConvert.SerializeObject(result));

        return response;
    }
}
