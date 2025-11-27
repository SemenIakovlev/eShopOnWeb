using System.Net;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Logging;

namespace DeliveryWorkerFunction;

public class OrdersFunction(Container container, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<OrdersFunction>();

    [Function("CreateOrder")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<OrderDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order is null || string.IsNullOrWhiteSpace(order.BuyerId) || order.OrderItems.Count == 0)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid order payload.");
                return bad;
            }
            var pk = new PartitionKey(order.BuyerId);
            var result = await container.CreateItemAsync(MapToCosmosOrder(order), pk);
            var resp = req.CreateResponse(HttpStatusCode.Created);
            await resp.WriteStringAsync(JsonSerializer.Serialize(new
            {
                id = result.Resource.Id,
                customerId = order.BuyerId,
                status = "Created"
            }));
            return resp;
        }
        catch (CosmosException cex)
        {
            _logger.LogError(cex, "Cosmos error");
            var resp = req.CreateResponse(cex.StatusCode);
            await resp.WriteStringAsync(cex.Message);
            return resp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
            await resp.WriteStringAsync("Internal error");
            return resp;
        }
    }

    private static CosmosOrderDto MapToCosmosOrder(OrderDto source)
    {
        return new CosmosOrderDto
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = source.Id,
            CustomerId = source.BuyerId,
            ShipToAddress = source.ShipToAddress,
            OrderItems = source.OrderItems,
            FinalPrice = Math.Round(source.OrderItems.Sum(i => i.UnitPrice * i.Units), 2),
        };
    }
}
