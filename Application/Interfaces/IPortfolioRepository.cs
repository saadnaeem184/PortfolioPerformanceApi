using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPortfolioRepository
    {
        // Portfolio Operations
        Task<Portfolio?> GetPortfolioByIdAsync(Guid id);
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync();
        Task AddPortfolioAsync(Portfolio portfolio);
        Task UpdatePortfolioAsync(Portfolio portfolio);
        Task DeletePortfolioAsync(Guid id);

        // Asset Operations
        Task<Asset?> GetAssetByIdAsync(Guid portfolioId, Guid assetId);
        Task<IEnumerable<Asset>> GetAssetsInPortfolioAsync(Guid portfolioId);
        Task AddAssetAsync(Guid portfolioId, Asset asset);
        Task UpdateAssetAsync(Guid portfolioId, Asset asset);
        Task DeleteAssetAsync(Guid portfolioId, Guid assetId);

        // Transaction Operations
        Task<Transaction?> GetTransactionByIdAsync(Guid assetId, Guid transactionId); // Added for completeness, though not heavily used by service
        Task<IEnumerable<Transaction>> GetTransactionsForAssetAsync(Guid assetId);
        Task AddTransactionAsync(Guid assetId, Transaction transaction);
        Task DeleteTransactionAsync(Guid assetId, Guid transactionId); // Added for completeness
    }
}
