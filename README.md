# PrimeBasket Backend Architecture

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)
![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![Architecture](https://img.shields.io/badge/Architecture-Microservices-orange)

PrimeBasket is a robust, highly scalable, and secure microservices-based e-commerce platform built with **.NET 8**. It leverages modern architectural patterns, including an API Gateway, event-driven messaging, and comprehensive code quality tooling.

---

## 🏗️ Architecture & Solution Structure

The project is divided into distinct, loosely coupled microservices, each possessing its own independent database context.

*   **PrimeBasket.ApiGateway** `(Port 5000)`: Built using **Ocelot**, this acts as the single point of entry for the frontend, routing requests to the appropriate downstream microservices.
*   **PrimeBasket.Auth.API** `(Port 5010)`: Handles user registration, login, JWT token generation, and role-based access control (Admin, Merchant, Customer).
*   **PrimeBasket.Cart.API** `(Port 5012)`: Manages shopping carts with inter-service HTTP calls to validate stock levels.
*   **PrimeBasket.Order.API** `(Port 5157)`: Processes checkouts, calculates order totals, and acts as the **RabbitMQ Event Producer** (`OrderPlacedEvent`).
*   **PrimeBasket.Payment.API** `(Port 5090)`: Manages digital wallets, transaction history, and acts as the **RabbitMQ Event Consumer** to asynchronously process successful orders.
*   **PrimeBasket.Product.API** `(Port 5209)`: Manages product catalogs, categories, and inventory tracking.
*   **PrimeBasket.Common**: A shared class library containing RabbitMQ configurations and event models (e.g., `OrderPlacedEvent`) to enforce strict contracts across services.
*   **PrimeBasket.UnitTests**: A centralized test project utilizing **NUnit**, **Moq**, and **Entity Framework InMemory** databases.

---

## 🛠️ Technology Stack

*   **Framework:** .NET 8.0 SDK
*   **Database:** SQL Server (Entity Framework Core)
*   **API Gateway:** Ocelot
*   **Message Broker:** RabbitMQ (Asynchronous Event-Driven Architecture)
*   **Security:** JWT (JSON Web Tokens) Authentication & Authorization
*   **Testing:** NUnit, Moq
*   **Code Quality:** SonarQube
*   **API Documentation & Testing:** Swagger (OpenAPI), Postman

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express edition recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Required to run RabbitMQ locally)

### 1. Start Infrastructure (RabbitMQ)
Ensure Docker is running, then spin up the RabbitMQ instance from the root directory:
```bash
docker-compose up -d
```
*   **RabbitMQ Server:** `localhost:5672`
*   **Management UI:** [http://localhost:15672](http://localhost:15672) *(Credentials: guest / guest)*

### 2. Launch All Microservices
A utility script is included to automatically boot all APIs in their own PowerShell instances:
```powershell
.\run-all.ps1
```
*(To shut them down cleanly later, simply run `.\stop-all.ps1`)*

---

## 🧪 Testing the APIs

### Option A: Postman (Recommended)
We have included a complete Postman collection right in the repository!
1. Open Postman and click **Import**.
2. Select the `PrimeBasket.postman_collection.json` file found in the root directory.
3. The collection is pre-configured with test scripts that automatically capture and inject your `JWT Bearer Token` when you run the **Login User** request, allowing you to seamlessly test the entire checkout flow!

### Option B: Swagger UI
Every individual service runs its own Swagger documentation.
- Auth API: [http://localhost:5010/swagger](http://localhost:5010/swagger)
- Product API: [http://localhost:5209/swagger](http://localhost:5209/swagger)
- Cart API: [http://localhost:5012/swagger](http://localhost:5012/swagger)
- Order API: [http://localhost:5157/swagger](http://localhost:5157/swagger)
- Payment API: [http://localhost:5090/swagger](http://localhost:5090/swagger)

*(Remember to click "Authorize" and paste your token as `Bearer eyJhbGci...` when interacting with secure endpoints).*

---

## 🔄 Verifying RabbitMQ Event Flow
To prove the asynchronous microservice communication is functioning:
1. Ensure both the **Order API** and **Payment API** are running.
2. Complete a successful checkout (via Swagger or Postman).
3. Check the console window running the **Payment API**. You will see the consumer intercepting the event:
   > `[RabbitMQ] Received OrderPlacedEvent: OrderId=1007, UserId=1, Amount=219.90`

---

## 🛡️ Code Quality & Unit Testing

### NUnit Test Suite
The project contains 30+ highly comprehensive unit tests covering all critical business logic, database operations, and inter-service HTTP mocking.
To execute the suite:
```powershell
dotnet test PrimeBasket.UnitTests\PrimeBasket.UnitTests.csproj
```

### SonarQube Integration
Static code analysis is integrated to maintain strict code quality gates. 
To run an analysis locally:
1. Ensure your local SonarQube server is running.
2. Execute the included PowerShell script:
```powershell
.\run-sonar.ps1
```
*(Requires Java 17+ and the `dotnet-sonarscanner` global tool).*
