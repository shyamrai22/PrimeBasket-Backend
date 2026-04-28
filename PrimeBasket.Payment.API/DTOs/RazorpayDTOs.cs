namespace PrimeBasket.Payments.API.DTOs;

public class RazorpayOrderRequest
{
    public decimal Amount { get; set; }
}

public class RazorpayOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class RazorpayVerifyRequest
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
