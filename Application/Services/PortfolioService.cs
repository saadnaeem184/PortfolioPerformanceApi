using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IMarketDataService _marketDataService;

        public PortfolioService(IPortfolioRepository portfolioRepository, IMarketDataService marketDataService)
        {
            _portfolioRepository = portfolioRepository;
            _marketDataService = marketDataService;
        }

        // --- Portfolio Operations ---

        /// <summary>
        /// Creates a new investment portfolio.
        /// </summary>
        /// <param name="createDto">Data for the new portfolio.</param>
        /// <returns>A DTO of the created portfolio, or null if creation failed.</returns>
        public async Task<PortfolioDto?> CreatePortfolioAsync(PortfolioCreateDto createDto)
        {
            var portfolio = new Portfolio
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                CreatedDate = DateTime.UtcNow
            };
            await _portfolioRepository.AddPortfolioAsync(portfolio);
            return new PortfolioDto { Id = portfolio.Id, Name = portfolio.Name, CreatedDate = portfolio.CreatedDate };
        }

        /// <summary>
        /// Retrieves all investment portfolios.
        /// </summary>
        /// <returns>An enumerable of PortfolioDto objects.</returns>
        public async Task<IEnumerable<PortfolioDto>> GetAllPortfoliosAsync()
        {
            var portfolios = await _portfolioRepository.GetAllPortfoliosAsync();
            return portfolios
                .Select(p => new PortfolioDto { Id = p.Id, Name = p.Name, CreatedDate = p.CreatedDate })
                .ToList();
        }

        /// <summary>
        /// Retrieves a specific investment portfolio by its ID.
        /// </summary>
        /// <param name="portfolioId">The unique identifier of the portfolio.</param>
        /// <returns>A detailed DTO of the portfolio, or null if not found.</returns>
        public async Task<PortfolioDetailDto?> GetPortfolioByIdAsync(Guid portfolioId)
        {
            var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(portfolioId);
            if (portfolio == null)
            {
                return null;
            }

            var assets = await _portfolioRepository.GetAssetsInPortfolioAsync(portfolioId);
            portfolio.Assets = assets.ToList();

            var portfolioDetailDto = new PortfolioDetailDto
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                CreatedDate = portfolio.CreatedDate,
                Assets = portfolio.Assets.Select(a => new AssetDto
                {
                    Id = a.Id,
                    PortfolioId = a.PortfolioId,
                    Symbol = a.Symbol,
                    Name = a.Name,
                    Type = a.Type
                }).ToList()
            };
            return portfolioDetailDto;
        }

        /// <summary>
        /// Updates an existing investment portfolio's details.
        /// </summary>
        /// <param name="portfolioId">The unique identifier of the portfolio to update.</param>
        /// <param name="updateDto">The updated data for the portfolio.</param>
        /// <returns>A DTO of the updated portfolio, or null if not found.</returns>
        public async Task<PortfolioDto?> UpdatePortfolioAsync(Guid portfolioId, PortfolioUpdateDto updateDto)
        {
            var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(portfolioId);
            if (portfolio == null)
            {
                return null;
            }
            portfolio.Name = updateDto.Name;
            await _portfolioRepository.UpdatePortfolioAsync(portfolio);
            return new PortfolioDto { Id = portfolio.Id, Name = portfolio.Name, CreatedDate = portfolio.CreatedDate };
        }

        /// <summary>
        /// Deletes an investment portfolio and all its associated assets and transactions.
        /// </summary>
        /// <param name="portfolioId">The unique identifier of the portfolio to delete.</param>
        /// <returns>True if deleted successfully, false if not found.</returns>
        public async Task<bool> DeletePortfolioAsync(Guid portfolioId)
        {
            var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(portfolioId);
            if (portfolio == null)
            {
                return false;
            }
            await _portfolioRepository.DeletePortfolioAsync(portfolioId);
            return true;
        }

        // --- Asset Operations ---

        /// <summary>
        /// Adds a new asset to a specific portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio to add the asset to.</param>
        /// <param name="createDto">Data for the new asset.</param>
        /// <returns>A DTO of the created asset, or null if portfolio not found or asset could not be added.</returns>
        public async Task<AssetDto?> AddAssetToPortfolioAsync(Guid portfolioId, AssetCreateDto createDto)
        {
            var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(portfolioId);
            if (portfolio == null)
            {
                return null; // Portfolio not found
            }

            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                Symbol = createDto.Symbol,
                Name = createDto.Name,
                Type = createDto.Type
            };
            await _portfolioRepository.AddAssetAsync(portfolioId, asset);
            return new AssetDto
            {
                Id = asset.Id,
                PortfolioId = asset.PortfolioId,
                Symbol = asset.Symbol,
                Name = asset.Name,
                Type = asset.Type
            };
        }

        /// <summary>
        /// Retrieves all assets belonging to a specific portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <returns>An enumerable of AssetDto objects.</returns>
        public async Task<IEnumerable<AssetDto>> GetAssetsInPortfolioAsync(Guid portfolioId)
        {
            var assets = await _portfolioRepository.GetAssetsInPortfolioAsync(portfolioId);
            return assets
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    PortfolioId = a.PortfolioId,
                    Symbol = a.Symbol,
                    Name = a.Name,
                    Type = a.Type
                })
                .ToList();
        }

        /// <summary>
        /// Retrieves a specific asset by its ID within a given portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="assetId">The ID of the asset.</param>
        /// <returns>A detailed DTO of the asset, or null if not found or not belonging to the portfolio.</returns>
        public async Task<AssetDto?> GetAssetByIdAsync(Guid portfolioId, Guid assetId)
        {
            var asset = await _portfolioRepository.GetAssetByIdAsync(portfolioId, assetId);
            if (asset == null)
            {
                return null;
            }

            var transactions = await _portfolioRepository.GetTransactionsForAssetAsync(asset.Id);
            asset.Transactions = transactions.ToList();

            var assetDetailDto = new AssetDetailDto
            {
                Id = asset.Id,
                PortfolioId = asset.PortfolioId,
                Symbol = asset.Symbol,
                Name = asset.Name,
                Type = asset.Type,
                Transactions = asset.Transactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    AssetId = t.AssetId,
                    Date = t.Date,
                    Quantity = t.Quantity,
                    Price = t.Price,
                    Type = t.Type
                }).ToList()
            };
            return assetDetailDto;
        }

        /// <summary>
        /// Updates an existing asset's details within a portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the asset.</param>
        /// <param name="assetId">The ID of the asset to update.</param>
        /// <param name="updateDto">The updated data for the asset.</param>
        /// <returns>A DTO of the updated asset, or null if not found or not belonging to the portfolio.</returns>
        public async Task<AssetDto?> UpdateAssetInPortfolioAsync(Guid portfolioId, Guid assetId, AssetUpdateDto updateDto)
        {
            var asset = await _portfolioRepository.GetAssetByIdAsync(portfolioId, assetId);
            if (asset == null)
            {
                return null;
            }
            asset.Symbol = updateDto.Symbol;
            asset.Name = updateDto.Name;
            asset.Type = updateDto.Type;
            await _portfolioRepository.UpdateAssetAsync(portfolioId, asset);
            return new AssetDto
            {
                Id = asset.Id,
                PortfolioId = asset.PortfolioId,
                Symbol = asset.Symbol,
                Name = asset.Name,
                Type = asset.Type
            };
        }

        /// <summary>
        /// Removes an asset from a portfolio, including all its associated transactions.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the asset.</param>
        /// <param name="assetId">The ID of the asset to remove.</param>
        /// <returns>True if removed successfully, false if not found or not belonging to the portfolio.</returns>
        public async Task<bool> RemoveAssetFromPortfolioAsync(Guid portfolioId, Guid assetId)
        {
            var asset = await _portfolioRepository.GetAssetByIdAsync(portfolioId, assetId);
            if (asset == null)
            {
                return false;
            }
            await _portfolioRepository.DeleteAssetAsync(portfolioId, assetId);
            return true;
        }

        // --- Transaction Operations ---

        /// <summary>
        /// Adds a new transaction for a specific asset within a portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="assetId">The ID of the asset to add the transaction to.</param>
        /// <param name="createDto">Data for the new transaction.</param>
        /// <returns>A DTO of the created transaction, or null if asset not found or not belonging to the portfolio.</returns>
        public async Task<TransactionDto?> AddTransactionToAssetAsync(Guid portfolioId, Guid assetId, TransactionCreateDto createDto)
        {
            var asset = await _portfolioRepository.GetAssetByIdAsync(portfolioId, assetId);
            if (asset == null)
            {
                return null; // Asset not found or not in specified portfolio
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AssetId = assetId,
                Date = createDto.Date.ToUniversalTime(),
                Quantity = createDto.Quantity,
                Price = createDto.Price,
                Type = createDto.Type
            };
            await _portfolioRepository.AddTransactionAsync(assetId, transaction);
            return new TransactionDto
            {
                Id = transaction.Id,
                AssetId = transaction.AssetId,
                Date = transaction.Date,
                Quantity = transaction.Quantity,
                Price = transaction.Price,
                Type = transaction.Type
            };
        }

        /// <summary>
        /// Retrieves all transactions for a specific asset within a portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="assetId">The ID of the asset.</param>
        /// <returns>An enumerable of TransactionDto objects, ordered by date.</returns>
        public async Task<IEnumerable<TransactionDto>> GetTransactionsForAssetAsync(Guid portfolioId, Guid assetId)
        {
            var asset = await _portfolioRepository.GetAssetByIdAsync(portfolioId, assetId);
            if (asset == null)
            {
                return Enumerable.Empty<TransactionDto>(); // Asset not found or not in specified portfolio
            }
            var transactions = await _portfolioRepository.GetTransactionsForAssetAsync(asset.Id);
            return transactions
                .OrderBy(t => t.Date)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    AssetId = t.AssetId,
                    Date = t.Date,
                    Quantity = t.Quantity,
                    Price = t.Price,
                    Type = t.Type
                })
                .ToList();
        }

        // --- Performance Reporting ---

        /// <summary>
        /// Calculates and retrieves the performance report for a specific portfolio over a specified date range.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="startDate">Optional: The start date for the performance calculation.</param>
        /// <param name="endDate">Optional: The end date for the performance calculation.</param>
        /// <returns>A PortfolioPerformanceDto object, or null if the portfolio is not found.</returns>
        public async Task<PortfolioPerformanceDto?> GetPortfolioPerformanceAsync(Guid portfolioId, DateTime? startDate, DateTime? endDate)
        {
            var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(portfolioId);
            if (portfolio == null)
            {
                return null;
            }

            var assetsInPortfolio = (await _portfolioRepository.GetAssetsInPortfolioAsync(portfolioId)).ToList();
            foreach (var asset in assetsInPortfolio)
            {
                asset.Transactions = (await _portfolioRepository.GetTransactionsForAssetAsync(asset.Id)).ToList();
            }
            portfolio.Assets = assetsInPortfolio;

            var performanceDto = new PortfolioPerformanceDto
            {
                PortfolioId = portfolio.Id,
                PortfolioName = portfolio.Name,
                AssetAllocation = new Dictionary<string, decimal>(),
                HistoricalValues = new List<HistoricalValueDto>()
            };

            decimal totalCurrentValue = 0;
            decimal totalRealizedGainLoss = 0;
            decimal totalUnrealizedGainLoss = 0;

            var assetPerformanceDetails = new List<AssetPerformanceDto>();

            foreach (var asset in portfolio.Assets)
            {
                var assetTransactions = asset.Transactions.OrderBy(t => t.Date).ToList();

                decimal currentQuantity = 0;
                decimal costBasisSum = 0;
                decimal realizedGainLoss = 0;

                var costBasisLots = new List<(decimal Quantity, decimal Price)>();

                foreach (var transaction in assetTransactions)
                {
                    if (transaction.Type == TransactionType.Buy)
                    {
                        currentQuantity += transaction.Quantity;
                        costBasisSum += transaction.Quantity * transaction.Price;
                        costBasisLots.Add((transaction.Quantity, transaction.Price)); // Add new lot to the end
                    }
                    else // TransactionType.Sell
                    {
                        decimal quantitySold = transaction.Quantity;
                        // Iterate through lots from the oldest (beginning of the list)
                        for (int i = 0; i < costBasisLots.Count && quantitySold > 0;)
                        {
                            var currentLot = costBasisLots[i];

                            if (quantitySold >= currentLot.Quantity)
                            {
                                // Sell entire current lot
                                realizedGainLoss += (transaction.Price - currentLot.Price) * currentLot.Quantity;
                                quantitySold -= currentLot.Quantity;
                                currentQuantity -= currentLot.Quantity;
                                costBasisSum -= currentLot.Quantity * currentLot.Price;
                                costBasisLots.RemoveAt(i); // Remove the consumed lot
                                
                            }
                            else
                            {
                                // Sell part of the current lot
                                realizedGainLoss += (transaction.Price - currentLot.Price) * quantitySold;
                                currentLot.Quantity -= quantitySold; // Modify quantity in place
                                costBasisLots[i] = currentLot; // Update the list with the modified tuple
                                currentQuantity -= quantitySold;
                                costBasisSum -= quantitySold * currentLot.Price;
                                quantitySold = 0; // All sold from this transaction
                                i++;
                            }
                        }
                        if (currentQuantity < 0) currentQuantity = 0;
                    }
                }

                decimal averageCostBasis = currentQuantity > 0 ? costBasisSum / currentQuantity : 0;
                decimal currentMarketPrice = await _marketDataService.GetCurrentPriceAsync(asset.Symbol);
                decimal unrealizedGainLoss = (currentMarketPrice - averageCostBasis) * currentQuantity;

                totalCurrentValue += currentQuantity * currentMarketPrice;
                totalRealizedGainLoss += realizedGainLoss;
                totalUnrealizedGainLoss += unrealizedGainLoss;

                assetPerformanceDetails.Add(new AssetPerformanceDto
                {
                    AssetId = asset.Id,
                    Symbol = asset.Symbol,
                    CurrentQuantity = currentQuantity,
                    AverageCostBasis = averageCostBasis,
                    CurrentMarketPrice = currentMarketPrice,
                    UnrealizedGainLoss = unrealizedGainLoss,
                    RealizedGainLoss = realizedGainLoss
                });
            }

            performanceDto.CurrentTotalValue = totalCurrentValue;
            performanceDto.TotalRealizedGainLoss = totalRealizedGainLoss;
            performanceDto.TotalUnrealizedGainLoss = totalUnrealizedGainLoss;

            if (totalCurrentValue > 0)
            {
                foreach (var assetPerf in assetPerformanceDetails)
                {
                    performanceDto.AssetAllocation[assetPerf.Symbol] = (assetPerf.CurrentQuantity * assetPerf.CurrentMarketPrice / totalCurrentValue) * 100;
                }
            }

            var effectiveStartDate = startDate ?? portfolio.CreatedDate.Date;
            var effectiveEndDate = endDate ?? DateTime.UtcNow.Date;

            for (DateTime date = effectiveStartDate; date <= effectiveEndDate; date = date.AddDays(1))
            {
                decimal dailyPortfolioValue = 0;
                foreach (var asset in portfolio.Assets)
                {
                    decimal quantityAtDate = 0;
                    foreach (var transaction in asset.Transactions.Where(t => t.Date.Date <= date.Date).OrderBy(t => t.Date))
                    {
                        if (transaction.Type == TransactionType.Buy)
                        {
                            quantityAtDate += transaction.Quantity;
                        }
                        else
                        {
                            quantityAtDate -= transaction.Quantity;
                        }
                    }
                    if (quantityAtDate < 0) quantityAtDate = 0;

                    decimal historicalPrice = await _marketDataService.GetCurrentPriceAsync(asset.Symbol);

                    dailyPortfolioValue += quantityAtDate * historicalPrice;
                }
                performanceDto.HistoricalValues.Add(new HistoricalValueDto { Date = date.Date, Value = dailyPortfolioValue });
            }

            return performanceDto;
        }
    }
}
