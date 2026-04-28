using Microsoft.EntityFrameworkCore;
using PrimeBasket.Payments.API.Data;
using PrimeBasket.Payments.API.DTOs;
using PrimeBasket.Payments.API.Entities;
using PrimeBasket.Payments.API.Interfaces;

using Razorpay.Api;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace PrimeBasket.Payments.API.Services;

public class PaymentService : IPaymentService
{
  private readonly PaymentDbContext _context;
  private readonly IConfiguration _configuration;

  public PaymentService(PaymentDbContext context, IConfiguration configuration)
  {
    _context = context;
    _configuration = configuration;
  }

  public async Task<PaymentResponse> ProcessPaymentAsync(int userId, PaymentRequest request)
  {
    var existingPayment = await _context.Payments
        .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey);

    if (existingPayment != null)
    {
      return new PaymentResponse
      {
        PaymentId = existingPayment.PaymentModelId,
        OrderId = existingPayment.OrderId,
        Amount = existingPayment.Amount,
        PaymentMethod = existingPayment.PaymentMethod,
        Status = existingPayment.Status,
        Message = "Payment already processed"
      };
    }

    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
      if (request.PaymentMethod == "COD")
      {
        var codPayment = new PaymentModel
        {
          UserId = userId,
          OrderId = request.OrderId,
          Amount = request.Amount,
          Status = "Pending",
          PaymentMethod = "COD",
          IdempotencyKey = request.IdempotencyKey,
          CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(codPayment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new PaymentResponse
        {
          PaymentId = codPayment.PaymentModelId,
          OrderId = codPayment.OrderId,
          Amount = codPayment.Amount,
          PaymentMethod = codPayment.PaymentMethod,
          Status = codPayment.Status,
          Message = "COD Payment initialized"
        };
      }

      var wallet = await _context.Wallets
          .Include(w => w.Transactions)
          .FirstOrDefaultAsync(w => w.UserId == userId);

      if (wallet == null)
        throw new Exception("Wallet not found");

      if (wallet.Balance < request.Amount)
      {
        return new PaymentResponse
        {
          OrderId = request.OrderId,
          Amount = request.Amount,
          PaymentMethod = request.PaymentMethod,
          Status = "Failed",
          Message = "Insufficient balance"
        };
      }

      wallet.Balance -= request.Amount;

      var txn = new TransactionModel
      {
        WalletId = wallet.WalletModelId,
        Amount = request.Amount,
        Type = "DEBIT",
        Status = "Success",
        CreatedAt = DateTime.UtcNow
      };

      await _context.Transactions.AddAsync(txn);

      var payment = new PaymentModel
      {
        UserId = userId,
        OrderId = request.OrderId,
        Amount = request.Amount,
        Status = "Success",
        PaymentMethod = "Wallet",
        IdempotencyKey = request.IdempotencyKey,
        CreatedAt = DateTime.UtcNow
      };

      await _context.Payments.AddAsync(payment);

      await _context.SaveChangesAsync();
      await transaction.CommitAsync();

      return new PaymentResponse
      {
        PaymentId = payment.PaymentModelId,
        OrderId = payment.OrderId,
        Amount = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        Status = payment.Status,
        Message = "Wallet Payment successful"
      };
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }


  public async Task<WalletResponse> CreateWalletAsync(int userId)
  {
    var existing = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
    if (existing != null)
      throw new Exception("Wallet already exists");

    var wallet = new WalletModel
    {
      UserId = userId,
      Balance = 0
    };

    await _context.Wallets.AddAsync(wallet);
    await _context.SaveChangesAsync();

    return new WalletResponse
    {
      WalletId = wallet.WalletModelId,
      Balance = wallet.Balance
    };
  }

  public async Task<WalletResponse> GetWalletByUserIdAsync(int userId)
  {
    var wallet = await _context.Wallets
        .FirstOrDefaultAsync(w => w.UserId == userId);

    if (wallet == null)
      throw new Exception("Wallet not found");

    return new WalletResponse
    {
      WalletId = wallet.WalletModelId,
      Balance = wallet.Balance
    };
  }

  public async Task<WalletResponse> AddMoneyAsync(int userId, AddMoneyRequest request)
  {
    var wallet = await _context.Wallets
        .FirstOrDefaultAsync(w => w.UserId == userId);

    if (wallet == null)
      throw new Exception("Wallet not found");

    wallet.Balance += request.Amount;

    var txn = new TransactionModel
    {
      WalletId = wallet.WalletModelId,
      Amount = request.Amount,
      Type = "CREDIT",
      Status = "Success",
      CreatedAt = DateTime.UtcNow
    };

    await _context.Transactions.AddAsync(txn);

    await _context.SaveChangesAsync();

    return new WalletResponse
    {
      WalletId = wallet.WalletModelId,
      Balance = wallet.Balance
    };
  }

  public async Task<List<TransactionResponse>> GetTransactionsAsync(int userId)
  {
    var wallet = await _context.Wallets
        .Include(w => w.Transactions)
        .FirstOrDefaultAsync(w => w.UserId == userId);

    if (wallet == null)
      throw new Exception("Wallet not found");

    return wallet.Transactions.Select(t => new TransactionResponse
    {
      TransactionId = t.TransactionModelId,
      Amount = t.Amount,
      Type = t.Type,
      Status = t.Status,
      CreatedAt = t.CreatedAt
    }).ToList();
  }

  public async Task<RazorpayOrderResponse> CreateRazorpayOrderAsync(int userId, RazorpayOrderRequest request)
  {
    var keyId = _configuration["Razorpay:KeyId"];
    var keySecret = _configuration["Razorpay:KeySecret"];

    var client = new RazorpayClient(keyId, keySecret);

    var options = new Dictionary<string, object>
    {
      { "amount", request.Amount * 100 }, // Razorpay expects amount in paise
      { "currency", "INR" },
      { "receipt", $"rcpt_{userId}_{DateTime.UtcNow.Ticks}" }
    };

    var order = client.Order.Create(options);

    return new RazorpayOrderResponse
    {
      OrderId = order["id"].ToString(),
      Amount = request.Amount,
      Currency = "INR"
    };
  }

  public async Task<WalletResponse> VerifyRazorpayPaymentAsync(int userId, RazorpayVerifyRequest request)
  {
    var keySecret = _configuration["Razorpay:KeySecret"];

    string payload = request.RazorpayOrderId + "|" + request.RazorpayPaymentId;
    
    using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret)))
    {
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

        if (generatedSignature != request.RazorpaySignature)
            throw new Exception("Invalid Payment Signature. Recharge failed.");
    }

    // Signature matches! It's an authentic payment.
    return await AddMoneyAsync(userId, new AddMoneyRequest { Amount = request.Amount });
  }
}