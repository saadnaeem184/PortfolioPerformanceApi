using Application.DTOs;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace PortfolioPerformanceApi.Tests.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {

    }

    public class PortfoliosControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PortfoliosControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostPortfolio_ReturnsCreatedAndPortfolio()
        {
            // Arrange
            var portfolioName = "Integration Test Portfolio";
            var createDto = new PortfolioCreateDto { Name = portfolioName };
            var content = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Portfolios", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var portfolioDto = JsonConvert.DeserializeObject<PortfolioDto>(responseString);

            Assert.NotNull(portfolioDto);
            Assert.NotEqual(Guid.Empty, portfolioDto.Id);
            Assert.Equal(portfolioName, portfolioDto.Name);
        }

        [Fact]
        public async Task GetPortfolios_ReturnsOkAndListOfPortfolios()
        {
            // Arrange: First, create a portfolio to ensure there's data
            var createDto = new PortfolioCreateDto { Name = "Another Test Portfolio" };
            var createContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/Portfolios", createContent);

            // Act
            var response = await _client.GetAsync("/api/Portfolios");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var portfolios = JsonConvert.DeserializeObject<List<PortfolioDto>>(responseString);

            Assert.NotNull(portfolios);
            Assert.True(portfolios.Any());
            Assert.Contains(portfolios, p => p.Name == "Another Test Portfolio");
        }

        [Fact]
        public async Task GetPortfolioById_ReturnsOk_WhenPortfolioExists()
        {
            // Arrange: Create a portfolio first
            var createDto = new PortfolioCreateDto { Name = "Specific Portfolio" };
            var createContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("/api/Portfolios", createContent);
            postResponse.EnsureSuccessStatusCode();
            var createdPortfolio = JsonConvert.DeserializeObject<PortfolioDto>(await postResponse.Content.ReadAsStringAsync());

            // Act
            var getResponse = await _client.GetAsync($"/api/Portfolios/{createdPortfolio.Id}");

            // Assert
            getResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var portfolioDetail = JsonConvert.DeserializeObject<PortfolioDetailDto>(await getResponse.Content.ReadAsStringAsync());
            Assert.NotNull(portfolioDetail);
            Assert.Equal(createdPortfolio.Id, portfolioDetail.Id);
            Assert.Equal(createdPortfolio.Name, portfolioDetail.Name);
        }

        [Fact]
        public async Task GetPortfolioById_ReturnsNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Portfolios/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeletePortfolio_ReturnsNoContent_WhenPortfolioExists()
        {
            // Arrange: Create a portfolio first
            var createDto = new PortfolioCreateDto { Name = "Portfolio to Delete" };
            var createContent = new StringContent(JsonConvert.SerializeObject(createDto), Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("/api/Portfolios", createContent);
            var createdPortfolio = JsonConvert.DeserializeObject<PortfolioDto>(await postResponse.Content.ReadAsStringAsync());

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/Portfolios/{createdPortfolio.Id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify it's actually deleted
            var getResponse = await _client.GetAsync($"/api/Portfolios/{createdPortfolio.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task AddAssetToPortfolio_ReturnsCreatedAndAsset()
        {
            // Arrange: Create a portfolio first
            var portfolioCreateDto = new PortfolioCreateDto { Name = "Portfolio For Asset" };
            var portfolioContent = new StringContent(JsonConvert.SerializeObject(portfolioCreateDto), Encoding.UTF8, "application/json");
            var portfolioResponse = await _client.PostAsync("/api/Portfolios", portfolioContent);
            var createdPortfolio = JsonConvert.DeserializeObject<PortfolioDto>(await portfolioResponse.Content.ReadAsStringAsync());

            var assetCreateDto = new AssetCreateDto { Symbol = "TESTA", Name = "Test Asset", Type = AssetType.Stock };
            var assetContent = new StringContent(JsonConvert.SerializeObject(assetCreateDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/api/Portfolios/{createdPortfolio.Id}/assets", assetContent);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var assetDto = JsonConvert.DeserializeObject<AssetDto>(responseString);

            Assert.NotNull(assetDto);
            Assert.NotEqual(Guid.Empty, assetDto.Id);
            Assert.Equal(assetCreateDto.Symbol, assetDto.Symbol);
            Assert.Equal(createdPortfolio.Id, assetDto.PortfolioId);
        }

        [Fact]
        public async Task GetPortfolioPerformance_ReturnsOkAndPerformanceData()
        {
            // Arrange: Create portfolio, add asset, add transactions
            var portfolioCreateDto = new PortfolioCreateDto { Name = "Performance Test Portfolio" };
            var portfolioContent = new StringContent(JsonConvert.SerializeObject(portfolioCreateDto), Encoding.UTF8, "application/json");
            var portfolioResponse = await _client.PostAsync("/api/Portfolios", portfolioContent);
            var createdPortfolio = JsonConvert.DeserializeObject<PortfolioDto>(await portfolioResponse.Content.ReadAsStringAsync());

            var assetCreateDto = new AssetCreateDto { Symbol = "PERF", Name = "Performance Stock", Type = AssetType.Stock };
            var assetContent = new StringContent(JsonConvert.SerializeObject(assetCreateDto), Encoding.UTF8, "application/json");
            var assetResponse = await _client.PostAsync($"/api/Portfolios/{createdPortfolio.Id}/assets", assetContent);
            var createdAsset = JsonConvert.DeserializeObject<AssetDto>(await assetResponse.Content.ReadAsStringAsync());

            var transactionBuyDto = new TransactionCreateDto { Date = DateTime.UtcNow.AddDays(-10), Quantity = 10, Price = 100m, Type = TransactionType.Buy };
            var transactionContent = new StringContent(JsonConvert.SerializeObject(transactionBuyDto), Encoding.UTF8, "application/json");
            await _client.PostAsync($"/api/Portfolios/{createdPortfolio.Id}/assets/{createdAsset.Id}/transactions", transactionContent);

            // Act
            var response = await _client.GetAsync($"/api/Portfolios/{createdPortfolio.Id}/performance");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var performanceDto = JsonConvert.DeserializeObject<PortfolioPerformanceDto>(responseString);

            Assert.NotNull(performanceDto);
            Assert.Equal(createdPortfolio.Id, performanceDto.PortfolioId);
            Assert.True(performanceDto.CurrentTotalValue > 0); // Should be non-zero if asset added and priced
            Assert.True(performanceDto.HistoricalValues.Any());
        }
    }
}
