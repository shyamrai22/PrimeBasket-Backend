# PrimeBasket Backend

PrimeBasket is a microservices-based e-commerce platform built with .NET 8.

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express edition recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Required for RabbitMQ)

## Getting Started

### 1. Start Infrastructure (RabbitMQ)
Open a terminal in the root directory and run:
```bash
docker-compose up -d
```
This will start RabbitMQ on `localhost:5672`. You can access the RabbitMQ Management UI at `http://localhost:15672` (Login: guest/guest).

### 2. Run All Microservices
We have provided a PowerShell script to launch all services automatically in separate windows:
```powershell
.\run-all.ps1
```

### 3. Verify RabbitMQ Integration
1. Place an order through the **Order API** (Swagger at `http://localhost:5169`).
2. Check the console window of the **Payment API**.
3. You should see a log message: `[RabbitMQ] Received OrderPlacedEvent: OrderId=...`, proving the asynchronous communication is working!

## Testing
To run the NUnit test suite:
```powershell
dotnet test PrimeBasket.UnitTests\PrimeBasket.UnitTests.csproj
```

## Solution Structure
- **PrimeBasket.Auth.API**: Authentication and Authorization (JWT).
- **PrimeBasket.Product.API**: Product management and stock.
- **PrimeBasket.Cart.API**: Shopping cart logic.
- **PrimeBasket.Order.API**: Order processing and history (Event Producer).
- **PrimeBasket.Payment.API**: Wallet management and payments (Event Consumer).
- **PrimeBasket.Common**: Shared messaging and event models.
- **PrimeBasket.ApiGateway**: Ocelot-based entry point for the frontend.
