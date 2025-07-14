using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices
{
    public class MarketDataService : IMarketDataService
    {
        // Simple mock for current prices. In a real app, this would hit an external API.
        private readonly Dictionary<string, decimal> _mockPrices = new Dictionary<string, decimal>
        {
            { "AAPL", 170.50m },
            { "MSFT", 320.75m },
            { "GOOGL", 135.20m },
            { "TSLA", 250.00m },
            { "AMZN", 140.00m }
        };

        /// <summary>
        /// Simulates fetching the current market price for a given asset symbol.
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., "AAPL").</param>
        /// <returns>The current simulated price.</returns>
        public Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            if (_mockPrices.TryGetValue(symbol.ToUpper(), out var price))
            {
                // Simulate some minor fluctuation for realism
                var random = new Random();
                var fluctuation = (decimal)(random.NextDouble() * 10 - 5); // +/- 5 units
                return Task.FromResult(price + fluctuation);
            }
            // If symbol not found, return a default price. In a production system,
            // this might throw a specific exception or return a structured error.
            return Task.FromResult(100.00m);
        }
    }
}
