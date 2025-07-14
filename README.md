# 📈 Portfolio Performance API

A **RESTful API** designed for a private investment company to manage client portfolios, track financial assets and transactions, and report on portfolio performance over time.

Built with **C# .NET Core** and adheres to **Clean Architecture** principles.

---

## 🚀 Features

- **Portfolio Management**: Create, retrieve, update, and delete investment portfolios.
- **Asset Management**: Add, update, and remove assets (stocks, bonds, funds) within a specific portfolio.
- **Transaction Tracking**: Record buy/sell transactions for assets (including date, quantity, and price).
- **Performance Reporting**:
  - Total current portfolio value
  - Realized and unrealized gains/losses (FIFO method)
  - Asset allocation breakdown
  - Historical portfolio value snapshots

---

## 🧱 Architecture

The project follows **Clean Architecture** (aka Onion/Hexagonal Architecture) for separation of concerns, maintainability, and testability.

### 📂 Layers

- **Domain Layer**:  
  Contains core business entities (`Portfolio`, `Asset`, `Transaction`) and fundamental rules. No dependencies.

- **Application Layer**:  
  Defines use cases and orchestrates domain entities. Includes:
  - DTOs for API contracts
  - Interfaces like `IPortfolioService`, `IMarketDataService`, `IPortfolioRepository`
  - Service logic in `PortfolioService`

- **Infrastructure Layer**:  
  Provides implementations for:
  - `IPortfolioRepository` → `InMemoryStore`
  - `IMarketDataService` → `MarketDataService`

This structure ensures that **business logic is independent of technologies**, making the system highly testable and adaptable.

---

## 🛠 Getting Started

### 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

---

## 📡 API Endpoints

### 🗂️ Portfolios

- GET /api/Portfolios
- GET /api/Portfolios/{id}
- POST /api/Portfolios
- PUT /api/Portfolios/{id}
- DELETE /api/Portfolios/{id}

---

### 📦 Assets (within a Portfolio)

- GET /api/Portfolios/{portfolioId}/assets
- GET /api/Portfolios/{portfolioId}/assets/{assetId}
- POST /api/Portfolios/{portfolioId}/assets
- PUT /api/Portfolios/{portfolioId}/assets/{assetId}
- DELETE /api/Portfolios/{portfolioId}/assets/{assetId}

---

### 💸 Transactions (within an Asset)

- GET /api/Portfolios/{portfolioId}/assets/{assetId}/transactions
- POST /api/Portfolios/{portfolioId}/assets/{assetId}/transactions

---

### 📊 Performance Reporting

- GET /api/Portfolios/{portfolioId}/performance?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd

---
