namespace DemoControllerApi;

public sealed record CreateOrderRequest(string ProductId, int Quantity);