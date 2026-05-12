using PrimeBasket.Payments.API.DTOs;

namespace PrimeBasket.Payments.API.Interfaces;

public interface IPaymentService
{
  Task<PaymentResponse> ProcessPaymentAsync(int userId, PaymentRequest request);
  Task<WalletResponse> CreateWalletAsync(int userId);
  Task<WalletResponse> GetWalletByUserIdAsync(int userId);
  Task<WalletResponse> AddMoneyAsync(int userId, AddMoneyRequest request);
  Task<List<TransactionResponse>> GetTransactionsAsync(int userId);
  Task<RazorpayOrderResponse> CreateRazorpayOrderAsync(int userId, RazorpayOrderRequest request);
  Task<WalletResponse> VerifyRazorpayPaymentAsync(int userId, RazorpayVerifyRequest request);
}