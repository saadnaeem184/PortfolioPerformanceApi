using Xunit;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Application.DTOs;
using Domain.Enums;

namespace PortfolioPerformanceApi.Tests.Tests.UnitTests
{
    public class PortfolioServiceTests
    {
        private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
        private readonly Mock<IMarketDataService> _mockMarketDataService;
        private readonly PortfolioService _portfolioService;

        public PortfolioServiceTests()
        {
            // Initialize mocks for dependencies
            _mockPortfolioRepository = new Mock<IPortfolioRepository>();
            _mockMarketDataService = new Mock<IMarketDataService>();

            // Create an instance of the service under test, injecting the mocks
            _portfolioService = new PortfolioService(_mockPortfolioRepository.Object, _mockMarketDataService.Object);
        }

        // --- Portfolio CRUD Tests ---

        [Fact]
        public async Task CreatePortfolioAsync_ShouldReturnPortfolioDto_WhenSuccessful()
        {
            // Arrange
            var createDto = new PortfolioCreateDto { Name = "New Test Portfolio" };
            _mockPortfolioRepository.Setup(repo => repo.AddPortfolioAsync(It.IsAny<Portfolio>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.CreatePortfolioAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.NotEqual(Guid.Empty, result.Id);
            _mockPortfolioRepository.Verify(repo => repo.AddPortfolioAsync(It.IsAny<Portfolio>()), Times.Once);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_ShouldReturnPortfolioDetailDto_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "Existing Portfolio" };
            var testAsset = new Asset { Id = Guid.NewGuid(), PortfolioId = portfolioId, Symbol = "TEST", Name = "Test Asset", Type = AssetType.Stock };
            testPortfolio.Assets.Add(testAsset); // Simulate asset linked to portfolio

            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.GetAssetsInPortfolioAsync(portfolioId))
                .ReturnsAsync(new List<Asset> { testAsset });

            // Act
            var result = await _portfolioService.GetPortfolioByIdAsync(portfolioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(portfolioId, result.Id);
            Assert.Equal(testPortfolio.Name, result.Name);
            Assert.Single(result.Assets);
            Assert.Equal(testAsset.Id, result.Assets.First().Id);
        }

        [Fact]
        public async Task DeletePortfolioAsync_ShouldReturnTrue_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "To Be Deleted" };

            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.DeletePortfolioAsync(portfolioId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.DeletePortfolioAsync(portfolioId);

            // Assert
            Assert.True(result);
            _mockPortfolioRepository.Verify(repo => repo.DeletePortfolioAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task AddAssetToPortfolioAsync_ShouldReturnAssetDto_WhenSuccessful()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var createDto = new AssetCreateDto { Symbol = "NEW", Name = "New Asset", Type = AssetType.Stock };
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "Portfolio" };

            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.AddAssetAsync(portfolioId, It.IsAny<Asset>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.AddAssetToPortfolioAsync(portfolioId, createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Symbol, result.Symbol);
            Assert.Equal(portfolioId, result.PortfolioId);
            _mockPortfolioRepository.Verify(repo => repo.AddAssetAsync(portfolioId, It.IsAny<Asset>()), Times.Once);
        }

        [Fact]
        public async Task AddTransactionToAssetAsync_ShouldReturnTransactionDto_WhenSuccessful()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var createDto = new TransactionCreateDto { Date = DateTime.UtcNow, Quantity = 10, Price = 100m, Type = TransactionType.Buy };
            var testAsset = new Asset { Id = assetId, PortfolioId = portfolioId, Symbol = "TEST", Name = "Test Asset", Type = AssetType.Stock };

            _mockPortfolioRepository.Setup(repo => repo.GetAssetByIdAsync(portfolioId, assetId))
                .ReturnsAsync(testAsset);
            _mockPortfolioRepository.Setup(repo => repo.AddTransactionAsync(assetId, It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _portfolioService.AddTransactionToAssetAsync(portfolioId, assetId, createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Quantity, result.Quantity);
            Assert.Equal(assetId, result.AssetId);
            _mockPortfolioRepository.Verify(repo => repo.AddTransactionAsync(assetId, It.IsAny<Transaction>()), Times.Once);
        }

        // --- Performance Calculation Tests ---

        [Fact]
        public async Task GetPortfolioPerformanceAsync_ShouldCalculateCorrectPerformance_SingleAssetBuy()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "Performance Portfolio" };
            var testAsset = new Asset { Id = assetId, PortfolioId = portfolioId, Symbol = "AAPL", Name = "Apple", Type = AssetType.Stock };
            var buyTransaction = new Transaction { Id = Guid.NewGuid(), AssetId = assetId, Date = DateTime.UtcNow.AddDays(-10), Quantity = 10, Price = 150m, Type = TransactionType.Buy };

            testAsset.Transactions.Add(buyTransaction); // Add directly for test setup convenience
            testPortfolio.Assets.Add(testAsset); // Add directly for test setup convenience

            // Mock repository calls
            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.GetAssetsInPortfolioAsync(portfolioId))
                .ReturnsAsync(new List<Asset> { testAsset });
            _mockPortfolioRepository.Setup(repo => repo.GetTransactionsForAssetAsync(assetId))
                .ReturnsAsync(new List<Transaction> { buyTransaction });

            // Mock market data
            _mockMarketDataService.Setup(m => m.GetCurrentPriceAsync("AAPL"))
                .ReturnsAsync(160m); // Current price is higher than buy price

            // Act
            var result = await _portfolioService.GetPortfolioPerformanceAsync(portfolioId, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(portfolioId, result.PortfolioId);
            Assert.Equal("Performance Portfolio", result.PortfolioName);
            Assert.Equal(10 * 160m, result.CurrentTotalValue); // 10 units * 160 current price = 1600
            Assert.Equal(0m, result.TotalRealizedGainLoss); // No sells yet
            Assert.Equal((160m - 150m) * 10, result.TotalUnrealizedGainLoss); // (Current - Avg Cost) * Qty = (160-150)*10 = 100
            Assert.Single(result.AssetAllocation);
            Assert.Equal(100m, result.AssetAllocation["AAPL"]); // Only one asset, so 100%
            Assert.True(result.HistoricalValues.Any()); // Should have historical values
        }

        [Fact]
        public async Task GetPortfolioPerformanceAsync_ShouldCalculateCorrectRealizedGainLoss()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "Test Portfolio" };
            var testAsset = new Asset { Id = assetId, PortfolioId = portfolioId, Symbol = "XYZ", Name = "XYZ Corp", Type = AssetType.Stock };

            var buy1 = new Transaction { Id = Guid.NewGuid(), AssetId = assetId, Date = DateTime.UtcNow.AddDays(-30), Quantity = 10, Price = 100m, Type = TransactionType.Buy };
            var buy2 = new Transaction { Id = Guid.NewGuid(), AssetId = assetId, Date = DateTime.UtcNow.AddDays(-20), Quantity = 5, Price = 110m, Type = TransactionType.Buy };
            var sell1 = new Transaction { Id = Guid.NewGuid(), AssetId = assetId, Date = DateTime.UtcNow.AddDays(-10), Quantity = 8, Price = 120m, Type = TransactionType.Sell }; // Sell 8 from first 10 @ 100
            var sell2 = new Transaction { Id = Guid.NewGuid(), AssetId = assetId, Date = DateTime.UtcNow.AddDays(-5), Quantity = 4, Price = 130m, Type = TransactionType.Sell }; // Sell remaining 2 from first 10 @ 100, then 2 from 5 @ 110

            var transactions = new List<Transaction> { buy1, buy2, sell1, sell2 };
            testAsset.Transactions.AddRange(transactions);
            testPortfolio.Assets.Add(testAsset);

            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.GetAssetsInPortfolioAsync(portfolioId))
                .ReturnsAsync(new List<Asset> { testAsset });
            _mockPortfolioRepository.Setup(repo => repo.GetTransactionsForAssetAsync(assetId))
                .ReturnsAsync(transactions);

            _mockMarketDataService.Setup(m => m.GetCurrentPriceAsync("XYZ"))
                .ReturnsAsync(140m); // Current price for remaining 1 unit (5-2-2=1)

            // Act
            var result = await _portfolioService.GetPortfolioPerformanceAsync(portfolioId, null, null);

            // Assert
            Assert.NotNull(result);

            // Realized Gain/Loss Calculation:
            // Sell 1 (8 units @ 120m, cost 100m): (120 - 100) * 8 = 20 * 8 = 160m
            // Sell 2 (4 units @ 130m):
            //   - First 2 units from remaining buy1 lot (cost 100m): (130 - 100) * 2 = 30 * 2 = 60m
            //   - Next 2 units from buy2 lot (cost 110m): (130 - 110) * 2 = 20 * 2 = 40m
            // Total Realized: 160 + 60 + 40 = 260m
            Assert.Equal(260m, result.TotalRealizedGainLoss);

            // Remaining quantity: 10 (buy1) + 5 (buy2) - 8 (sell1) - 4 (sell2) = 3 units
            // These 3 units are from buy2 (cost 110m)
            // Unrealized Gain/Loss: (140m - 110m) * 3 = 30 * 3 = 90m
            Assert.Equal(90m, result.TotalUnrealizedGainLoss);

            // Current Total Value: Remaining 3 units * 140m current price = 420m
            Assert.Equal(420m, result.CurrentTotalValue);
        }

        [Fact]
        public async Task GetPortfolioPerformanceAsync_ShouldReturnZeroForEmptyPortfolio()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "Empty Portfolio" };

            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.GetAssetsInPortfolioAsync(portfolioId))
                .ReturnsAsync(new List<Asset>()); // No assets

            // Act
            var result = await _portfolioService.GetPortfolioPerformanceAsync(portfolioId, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.CurrentTotalValue);
            Assert.Equal(0m, result.TotalRealizedGainLoss);
            Assert.Equal(0m, result.TotalUnrealizedGainLoss);
            Assert.Empty(result.AssetAllocation);
            Assert.True(result.HistoricalValues.Any()); // Still generates historical points, but values should be 0
            Assert.All(result.HistoricalValues, hv => Assert.Equal(0m, hv.Value));
        }

        [Fact]
        public async Task GetPortfolioPerformanceAsync_ShouldFilterHistoricalValuesByDateRange()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var testPortfolio = new Portfolio { Id = portfolioId, Name = "Date Range Portfolio", CreatedDate = DateTime.UtcNow.AddDays(-50) };
            var testAsset = new Asset { Id = assetId, PortfolioId = portfolioId, Symbol = "DAT", Name = "Date Asset", Type = AssetType.Stock };
            testAsset.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AssetId = assetId, Date = DateTime.UtcNow.AddDays(-40), Quantity = 10, Price = 50m, Type = TransactionType.Buy });
            testPortfolio.Assets.Add(testAsset);

            _mockPortfolioRepository.Setup(repo => repo.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(testPortfolio);
            _mockPortfolioRepository.Setup(repo => repo.GetAssetsInPortfolioAsync(portfolioId))
                .ReturnsAsync(new List<Asset> { testAsset });
            _mockPortfolioRepository.Setup(repo => repo.GetTransactionsForAssetAsync(assetId))
                .ReturnsAsync(testAsset.Transactions);
            _mockMarketDataService.Setup(m => m.GetCurrentPriceAsync("DAT"))
                .ReturnsAsync(60m);

            var startDate = DateTime.UtcNow.AddDays(-20);
            var endDate = DateTime.UtcNow.AddDays(-10);

            // Act
            var result = await _portfolioService.GetPortfolioPerformanceAsync(portfolioId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.HistoricalValues, hv =>
            {
                Assert.True(hv.Date.Date >= startDate.Date);
                Assert.True(hv.Date.Date <= endDate.Date);
            });
            // Verify the number of historical points matches the date range
            Assert.Equal((int)(endDate.Date - startDate.Date).TotalDays + 1, result.HistoricalValues.Count);
        }
    }
}
