using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = Domain.Entities.Transaction;

namespace Infrastructure.Data
{
    public class InMemoryStore : IPortfolioRepository
    {
        // Concurrent dictionaries for thread-safe access in a multi-threaded environment (like a web server)
        private readonly ConcurrentDictionary<Guid, Portfolio> _portfolios = new ConcurrentDictionary<Guid, Portfolio>();
        private readonly ConcurrentDictionary<Guid, Asset> _assets = new ConcurrentDictionary<Guid, Asset>();
        private readonly ConcurrentDictionary<Guid, Transaction> _transactions = new ConcurrentDictionary<Guid, Transaction>();

        public InMemoryStore()
        {
            SeedData();
        }

        private void SeedData()
        {
            var portfolio1Id = Guid.NewGuid();
            var portfolio2Id = Guid.NewGuid();

            var portfolio1 = new Portfolio { Id = portfolio1Id, Name = "My Growth Portfolio", CreatedDate = DateTime.UtcNow.AddDays(-30) };
            var portfolio2 = new Portfolio { Id = portfolio2Id, Name = "Retirement Savings", CreatedDate = DateTime.UtcNow.AddDays(-60) };

            _portfolios.TryAdd(portfolio1.Id, portfolio1);
            _portfolios.TryAdd(portfolio2.Id, portfolio2);

            var asset1Id = Guid.NewGuid(); // AAPL
            var asset2Id = Guid.NewGuid(); // MSFT
            var asset3Id = Guid.NewGuid(); // GOOGL (in portfolio 2)

            var asset1 = new Asset { Id = asset1Id, PortfolioId = portfolio1Id, Symbol = "AAPL", Name = "Apple Inc.", Type = AssetType.Stock };
            var asset2 = new Asset { Id = asset2Id, PortfolioId = portfolio1Id, Symbol = "MSFT", Name = "Microsoft Corp.", Type = AssetType.Stock };
            var asset3 = new Asset { Id = asset3Id, PortfolioId = portfolio2Id, Symbol = "GOOGL", Name = "Alphabet Inc. (Class A)", Type = AssetType.Stock };

            _assets.TryAdd(asset1.Id, asset1);
            _assets.TryAdd(asset2.Id, asset2);
            _assets.TryAdd(asset3.Id, asset3);

            portfolio1.Assets.Add(asset1);
            portfolio1.Assets.Add(asset2);
            portfolio2.Assets.Add(asset3);

            // Transactions for AAPL
            var trans1_aapl_buy1 = new Transaction { Id = Guid.NewGuid(), AssetId = asset1Id, Date = DateTime.UtcNow.AddDays(-25), Quantity = 10, Price = 150.00m, Type = TransactionType.Buy };
            var trans2_aapl_buy2 = new Transaction { Id = Guid.NewGuid(), AssetId = asset1Id, Date = DateTime.UtcNow.AddDays(-20), Quantity = 5, Price = 155.00m, Type = TransactionType.Buy };
            var trans3_aapl_sell1 = new Transaction { Id = Guid.NewGuid(), AssetId = asset1Id, Date = DateTime.UtcNow.AddDays(-10), Quantity = 3, Price = 160.00m, Type = TransactionType.Sell };
            var trans4_aapl_buy3 = new Transaction { Id = Guid.NewGuid(), AssetId = asset1Id, Date = DateTime.UtcNow.AddDays(-5), Quantity = 7, Price = 165.00m, Type = TransactionType.Buy };

            _transactions.TryAdd(trans1_aapl_buy1.Id, trans1_aapl_buy1);
            _transactions.TryAdd(trans2_aapl_buy2.Id, trans2_aapl_buy2);
            _transactions.TryAdd(trans3_aapl_sell1.Id, trans3_aapl_sell1);
            _transactions.TryAdd(trans4_aapl_buy3.Id, trans4_aapl_buy3);

            asset1.Transactions.Add(trans1_aapl_buy1);
            asset1.Transactions.Add(trans2_aapl_buy2);
            asset1.Transactions.Add(trans3_aapl_sell1);
            asset1.Transactions.Add(trans4_aapl_buy3);

            // Transactions for MSFT
            var trans1_msft_buy1 = new Transaction { Id = Guid.NewGuid(), AssetId = asset2Id, Date = DateTime.UtcNow.AddDays(-28), Quantity = 20, Price = 300.00m, Type = TransactionType.Buy };
            var trans2_msft_buy2 = new Transaction { Id = Guid.NewGuid(), AssetId = asset2Id, Date = DateTime.UtcNow.AddDays(-15), Quantity = 10, Price = 310.00m, Type = TransactionType.Buy };

            _transactions.TryAdd(trans1_msft_buy1.Id, trans1_msft_buy1);
            _transactions.TryAdd(trans2_msft_buy2.Id, trans2_msft_buy2);

            asset2.Transactions.Add(trans1_msft_buy1);
            asset2.Transactions.Add(trans2_msft_buy2);

            // Transactions for GOOGL
            var trans1_googl_buy1 = new Transaction { Id = Guid.NewGuid(), AssetId = asset3Id, Date = DateTime.UtcNow.AddDays(-50), Quantity = 5, Price = 120.00m, Type = TransactionType.Buy };

            _transactions.TryAdd(trans1_googl_buy1.Id, trans1_googl_buy1);
            asset3.Transactions.Add(trans1_googl_buy1);
        }

        // --- IPortfolioRepository Implementation ---

        public Task<Portfolio?> GetPortfolioByIdAsync(Guid id)
        {
            _portfolios.TryGetValue(id, out var portfolio);
            return Task.FromResult(portfolio);
        }

        public Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync()
        {
            return Task.FromResult<IEnumerable<Portfolio>>(_portfolios.Values.ToList());
        }

        public Task AddPortfolioAsync(Portfolio portfolio)
        {
            _portfolios.TryAdd(portfolio.Id, portfolio);
            return Task.CompletedTask;
        }

        public Task UpdatePortfolioAsync(Portfolio portfolio)
        {
            // In-memory update is direct as we modify the existing object
            _portfolios.AddOrUpdate(portfolio.Id, portfolio, (key, existingVal) => portfolio);
            return Task.CompletedTask;
        }

        public Task DeletePortfolioAsync(Guid id)
        {
            if (_portfolios.TryRemove(id, out var portfolio))
            {
                // Remove associated assets and transactions
                var assetsToRemove = _assets.Values.Where(a => a.PortfolioId == id).ToList();
                foreach (var asset in assetsToRemove)
                {
                    _assets.TryRemove(asset.Id, out _);
                    var transactionsToRemove = _transactions.Values.Where(t => t.AssetId == asset.Id).ToList();
                    foreach (var transaction in transactionsToRemove)
                    {
                        _transactions.TryRemove(transaction.Id, out _);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task<Asset?> GetAssetByIdAsync(Guid portfolioId, Guid assetId)
        {
            _assets.TryGetValue(assetId, out var asset);
            // Ensure the asset belongs to the specified portfolio
            return Task.FromResult(asset?.PortfolioId == portfolioId ? asset : null);
        }

        public Task<IEnumerable<Asset>> GetAssetsInPortfolioAsync(Guid portfolioId)
        {
            return Task.FromResult<IEnumerable<Asset>>(_assets.Values.Where(a => a.PortfolioId == portfolioId).ToList());
        }

        public Task AddAssetAsync(Guid portfolioId, Asset asset)
        {
            if (_portfolios.TryGetValue(portfolioId, out var portfolio))
            {
                _assets.TryAdd(asset.Id, asset);
                portfolio.Assets.Add(asset); // Maintain the in-memory graph
            }
            return Task.CompletedTask;
        }

        public Task UpdateAssetAsync(Guid portfolioId, Asset asset)
        {
            // Ensure the asset belongs to the specified portfolio before updating
            if (_assets.TryGetValue(asset.Id, out var existingAsset) && existingAsset.PortfolioId == portfolioId)
            {
                _assets.AddOrUpdate(asset.Id, asset, (key, existingVal) => asset);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAssetAsync(Guid portfolioId, Guid assetId)
        {
            if (_assets.TryRemove(assetId, out var assetToRemove) && assetToRemove.PortfolioId == portfolioId)
            {
                // Remove from portfolio's asset list
                if (_portfolios.TryGetValue(portfolioId, out var portfolio))
                {
                    portfolio.Assets.Remove(assetToRemove);
                }
                // Remove all associated transactions
                var transactionsToRemove = _transactions.Values.Where(t => t.AssetId == assetId).ToList();
                foreach (var transaction in transactionsToRemove)
                {
                    _transactions.TryRemove(transaction.Id, out _);
                }
            }
            return Task.CompletedTask;
        }

        public Task<Transaction?> GetTransactionByIdAsync(Guid assetId, Guid transactionId)
        {
            _transactions.TryGetValue(transactionId, out var transaction);
            // Ensure the transaction belongs to the specified asset
            return Task.FromResult(transaction?.AssetId == assetId ? transaction : null);
        }

        public Task<IEnumerable<Transaction>> GetTransactionsForAssetAsync(Guid assetId)
        {
            return Task.FromResult<IEnumerable<Transaction>>(_transactions.Values.Where(t => t.AssetId == assetId).ToList());
        }

        public Task AddTransactionAsync(Guid assetId, Transaction transaction)
        {
            if (_assets.TryGetValue(assetId, out var asset))
            {
                _transactions.TryAdd(transaction.Id, transaction);
                asset.Transactions.Add(transaction); // Maintain the in-memory graph
            }
            return Task.CompletedTask;
        }

        public Task DeleteTransactionAsync(Guid assetId, Guid transactionId)
        {
            if (_transactions.TryRemove(transactionId, out var transactionToRemove) && transactionToRemove.AssetId == assetId)
            {
                // Remove from asset's transaction list
                if (_assets.TryGetValue(assetId, out var asset))
                {
                    asset.Transactions.Remove(transactionToRemove);
                }
            }
            return Task.CompletedTask;
        }
    }
}
