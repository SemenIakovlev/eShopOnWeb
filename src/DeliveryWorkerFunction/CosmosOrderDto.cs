using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace DeliveryWorkerFunction;

public class CosmosOrderDto
{
    public string Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public AddressDto ShipToAddress { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
}

