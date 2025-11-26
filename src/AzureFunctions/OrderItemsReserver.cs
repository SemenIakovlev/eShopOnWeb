using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;

namespace AzureFunctions;

public static class OrderItemsReserver
{
    [Function("OrderItemsReserver")]
    public static async Task<OrderResponse> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync() ?? string.Empty;
        var order = JsonConvert.DeserializeObject<OrderDto>(body);
        if (order == null)
        {
            var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid order data.");
            return new OrderResponse
            {
                Id = 0,
                OrderJson = string.Empty,
                HttpResponse = badResponse
            };
        }

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync($"Order {order.Id} saved.");

        return new OrderResponse
        {
            Id = order.Id,
            OrderJson = JsonConvert.SerializeObject(order),
            HttpResponse = response
        };
    }
}
