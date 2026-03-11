using Idempotency.Net.AspNetCore;

using Microsoft.AspNetCore.Mvc;

namespace DemoControllerApi.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost]
    [Idempotent]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new
        {
            Id = Guid.NewGuid(),
            request.ProductId,
            request.Quantity,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return Ok(order);
    }
}