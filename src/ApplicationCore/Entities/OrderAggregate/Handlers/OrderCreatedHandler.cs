using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Handlers;

public class OrderCreatedHandler(
    ILogger<OrderCreatedHandler> logger,
    IEmailSender emailSender,
    IHttpClientFactory httpClientFactory)
    : INotificationHandler<OrderCreatedEvent>
{
    private const string AzureFunctionUrl = 
        "https://order-items-reserver.azurewebsites.net/api/OrderItemsReserver?code=JPRWVbSX5x6l_ds8hJS1B5JE-oqFg3GQo6QN1NAHHlbsAzFuPlM8Dg==";

    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order {OrderId} placed", domainEvent.Order.Id);

        await emailSender.SendEmailAsync("to@test.com",
                                         "Order Created",
                                         $"Order with id {domainEvent.Order.Id} was created.");

        var orderDto = new OrderDto
        {
            Id = domainEvent.Order.Id,
            BuyerId = domainEvent.Order.BuyerId,
            OrderDate = domainEvent.Order.OrderDate,
            ShipToAddress = new AddressDto
            {
                Street = domainEvent.Order.ShipToAddress.Street,
                City = domainEvent.Order.ShipToAddress.City,
                State = domainEvent.Order.ShipToAddress.State,
                Country = domainEvent.Order.ShipToAddress.Country,
                ZipCode = domainEvent.Order.ShipToAddress.ZipCode
            },
            OrderItems = domainEvent.Order.OrderItems.Select(oi => new OrderItemDto
            {
                ItemOrdered = new CatalogItemOrderedDto
                {
                    CatalogItemId = oi.ItemOrdered.CatalogItemId,
                    ProductName = oi.ItemOrdered.ProductName,
                    PictureUri = oi.ItemOrdered.PictureUri
                },
                UnitPrice = oi.UnitPrice,
                Units = oi.Units
            }).ToList()
        };

        var json = JsonSerializer.Serialize(orderDto);
        var client = httpClientFactory.CreateClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await client.PostAsync(AzureFunctionUrl, content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"Order sent to Azure Function successfully. OrderId: {domainEvent.Order.Id}");
            }
            else
            {
                logger.LogError($"Failed to send order to Azure Function. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while sending order to Azure Function");
        }
    }
}
