using Microsoft.EntityFrameworkCore;
using PrimeBasket.Payments.API.Entities;

namespace PrimeBasket.Payments.API.Data;

public class PaymentDbContext : DbContext
{
  public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
      : base(options) { }

  public DbSet<PaymentModel> Payments => Set<PaymentModel>();
  public DbSet<WalletModel> Wallets => Set<WalletModel>();
  public DbSet<TransactionModel> Transactions => Set<TransactionModel>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // ---------------- PAYMENT ----------------
    modelBuilder.Entity<PaymentModel>()
        .Property(p => p.Amount)
        .HasPrecision(18, 2);

    modelBuilder.Entity<PaymentModel>()
        .HasIndex(p => p.UserId);

    modelBuilder.Entity<PaymentModel>()
        .HasIndex(p => p.IdempotencyKey)
        .IsUnique(); // Prevent duplicate payments

    // ---------------- WALLET ----------------
    modelBuilder.Entity<WalletModel>()
        .Property(w => w.Balance)
        .HasPrecision(18, 2);

    modelBuilder.Entity<WalletModel>()
        .HasIndex(w => w.UserId)
        .IsUnique(); // One wallet per user

    // ---------------- TRANSACTION ----------------
    modelBuilder.Entity<TransactionModel>()
        .Property(t => t.Amount)
        .HasPrecision(18, 2);

    modelBuilder.Entity<TransactionModel>()
        .HasOne(t => t.Wallet)
        .WithMany(w => w.Transactions)
        .HasForeignKey(t => t.WalletId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}