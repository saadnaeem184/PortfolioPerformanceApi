using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPortfolioService
    {
        // Portfolio Operations
        Task<PortfolioDto?> CreatePortfolioAsync(PortfolioCreateDto createDto);
        Task<IEnumerable<PortfolioDto>> GetAllPortfoliosAsync();
        Task<PortfolioDetailDto?> GetPortfolioByIdAsync(Guid portfolioId);
        Task<PortfolioDto?> UpdatePortfolioAsync(Guid portfolioId, PortfolioUpdateDto updateDto);
        Task<bool> DeletePortfolioAsync(Guid portfolioId);

        // Asset Operations
        Task<AssetDto?> AddAssetToPortfolioAsync(Guid portfolioId, AssetCreateDto createDto);
        Task<IEnumerable<AssetDto>> GetAssetsInPortfolioAsync(Guid portfolioId);
        Task<AssetDto?> GetAssetByIdAsync(Guid portfolioId, Guid assetId);
        Task<AssetDto?> UpdateAssetInPortfolioAsync(Guid portfolioId, Guid assetId, AssetUpdateDto updateDto);
        Task<bool> RemoveAssetFromPortfolioAsync(Guid portfolioId, Guid assetId);

        // Transaction Operations
        Task<TransactionDto?> AddTransactionToAssetAsync(Guid portfolioId, Guid assetId, TransactionCreateDto createDto);
        Task<IEnumerable<TransactionDto>> GetTransactionsForAssetAsync(Guid portfolioId, Guid assetId);

        // Performance Reporting
        Task<PortfolioPerformanceDto?> GetPortfolioPerformanceAsync(Guid portfolioId, DateTime? startDate, DateTime? endDate);
    }
}
