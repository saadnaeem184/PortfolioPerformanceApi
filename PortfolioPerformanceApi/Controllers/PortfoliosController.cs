using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioPerformanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfoliosController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfoliosController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        // GET: api/Portfolios
        /// <summary>
        /// Retrieves a list of all investment portfolios.
        /// </summary>
        /// <returns>A list of PortfolioDto objects.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PortfolioDto>), 200)]
        public async Task<IActionResult> GetAllPortfolios()
        {
            var portfolios = await _portfolioService.GetAllPortfoliosAsync();
            return Ok(portfolios);
        }

        // GET: api/Portfolios/{id}
        /// <summary>
        /// Retrieves a specific investment portfolio by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the portfolio.</param>
        /// <returns>A PortfolioDetailDto object if found, otherwise 404 Not Found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PortfolioDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPortfolioById(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                return NotFound();
            }
            return Ok(portfolio);
        }

        // POST: api/Portfolios
        /// <summary>
        /// Creates a new investment portfolio.
        /// </summary>
        /// <param name="createDto">The data for the new portfolio.</param>
        /// <returns>The created PortfolioDto with its ID, or 400 Bad Request if invalid.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PortfolioDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreatePortfolio([FromBody] PortfolioCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var portfolio = await _portfolioService.CreatePortfolioAsync(createDto);
            if (portfolio == null)
            {
                return BadRequest("Could not create portfolio."); // Should not happen with current in-memory store
            }
            return CreatedAtAction(nameof(GetPortfolioById), new { id = portfolio.Id }, portfolio);
        }

        // PUT: api/Portfolios/{id}
        /// <summary>
        /// Updates an existing investment portfolio.
        /// </summary>
        /// <param name="id">The unique identifier of the portfolio to update.</param>
        /// <param name="updateDto">The updated data for the portfolio.</param>
        /// <returns>The updated PortfolioDto if successful, 404 Not Found if portfolio doesn't exist, or 400 Bad Request if invalid.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PortfolioDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePortfolio(Guid id, [FromBody] PortfolioUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedPortfolio = await _portfolioService.UpdatePortfolioAsync(id, updateDto);
            if (updatedPortfolio == null)
            {
                return NotFound();
            }
            return Ok(updatedPortfolio);
        }

        // DELETE: api/Portfolios/{id}
        /// <summary>
        /// Deletes an investment portfolio by its ID, including all associated assets and transactions.
        /// </summary>
        /// <param name="id">The unique identifier of the portfolio to delete.</param>
        /// <returns>204 No Content if successful, or 404 Not Found if portfolio doesn't exist.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            var deleted = await _portfolioService.DeletePortfolioAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET: api/Portfolios/{portfolioId}/assets
        /// <summary>
        /// Retrieves all assets within a specific portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <returns>A list of AssetDto objects.</returns>
        [HttpGet("{portfolioId}/assets")]
        [ProducesResponseType(typeof(IEnumerable<AssetDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAssetsInPortfolio(Guid portfolioId)
        {
            // Check if portfolio exists first
            if (await _portfolioService.GetPortfolioByIdAsync(portfolioId) == null)
            {
                return NotFound($"Portfolio with ID {portfolioId} not found.");
            }
            var assets = await _portfolioService.GetAssetsInPortfolioAsync(portfolioId);
            return Ok(assets);
        }

        // GET: api/Portfolios/{portfolioId}/assets/{assetId}
        /// <summary>
        /// Retrieves a specific asset within a portfolio by its ID.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="assetId">The ID of the asset.</param>
        /// <returns>An AssetDetailDto object if found, otherwise 404 Not Found.</returns>
        [HttpGet("{portfolioId}/assets/{assetId}")]
        [ProducesResponseType(typeof(AssetDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAssetById(Guid portfolioId, Guid assetId)
        {
            var asset = await _portfolioService.GetAssetByIdAsync(portfolioId, assetId);
            if (asset == null)
            {
                return NotFound();
            }
            return Ok(asset);
        }

        // POST: api/Portfolios/{portfolioId}/assets
        /// <summary>
        /// Adds a new asset to a specific portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio to add the asset to.</param>
        /// <param name="createDto">The data for the new asset.</param>
        /// <returns>The created AssetDto with its ID, or 400 Bad Request if invalid/portfolio not found.</returns>
        [HttpPost("{portfolioId}/assets")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddAssetToPortfolio(Guid portfolioId, [FromBody] AssetCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var asset = await _portfolioService.AddAssetToPortfolioAsync(portfolioId, createDto);
            if (asset == null)
            {
                return NotFound($"Portfolio with ID {portfolioId} not found or asset could not be added.");
            }
            return CreatedAtAction(nameof(GetAssetById), new { portfolioId = portfolioId, assetId = asset.Id }, asset);
        }

        // PUT: api/Portfolios/{portfolioId}/assets/{assetId}
        /// <summary>
        /// Updates an existing asset within a portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the asset.</param>
        /// <param name="assetId">The ID of the asset to update.</param>
        /// <param name="updateDto">The updated data for the asset.</param>
        /// <returns>The updated AssetDto if successful, 404 Not Found if asset/portfolio doesn't exist, or 400 Bad Request if invalid.</returns>
        [HttpPut("{portfolioId}/assets/{assetId}")]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAssetInPortfolio(Guid portfolioId, Guid assetId, [FromBody] AssetUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedAsset = await _portfolioService.UpdateAssetInPortfolioAsync(portfolioId, assetId, updateDto);
            if (updatedAsset == null)
            {
                return NotFound($"Asset with ID {assetId} not found in portfolio {portfolioId}.");
            }
            return Ok(updatedAsset);
        }

        // DELETE: api/Portfolios/{portfolioId}/assets/{assetId}
        /// <summary>
        /// Removes an asset from a portfolio, including all its associated transactions.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the asset.</param>
        /// <param name="assetId">The ID of the asset to remove.</param>
        /// <returns>204 No Content if successful, or 404 Not Found if asset/portfolio doesn't exist.</returns>
        [HttpDelete("{portfolioId}/assets/{assetId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveAssetFromPortfolio(Guid portfolioId, Guid assetId)
        {
            var removed = await _portfolioService.RemoveAssetFromPortfolioAsync(portfolioId, assetId);
            if (!removed)
            {
                return NotFound($"Asset with ID {assetId} not found in portfolio {portfolioId}.");
            }
            return NoContent();
        }

        // POST: api/Portfolios/{portfolioId}/assets/{assetId}/transactions
        /// <summary>
        /// Adds a new transaction for a specific asset within a portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="assetId">The ID of the asset to add the transaction to.</param>
        /// <param name="createDto">The data for the new transaction.</param>
        /// <returns>The created TransactionDto with its ID, or 400 Bad Request if invalid/asset not found.</returns>
        [HttpPost("{portfolioId}/assets/{assetId}/transactions")]
        [ProducesResponseType(typeof(TransactionDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddTransactionToAsset(Guid portfolioId, Guid assetId, [FromBody] TransactionCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var transaction = await _portfolioService.AddTransactionToAssetAsync(portfolioId, assetId, createDto);
            if (transaction == null)
            {
                return NotFound($"Asset with ID {assetId} not found in portfolio {portfolioId}.");
            }
            return CreatedAtAction(nameof(GetTransactionsForAsset), new { portfolioId = portfolioId, assetId = assetId }, transaction);
        }

        // GET: api/Portfolios/{portfolioId}/assets/{assetId}/transactions
        /// <summary>
        /// Retrieves all transactions for a specific asset within a portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="assetId">The ID of the asset.</param>
        /// <returns>A list of TransactionDto objects.</returns>
        [HttpGet("{portfolioId}/assets/{assetId}/transactions")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTransactionsForAsset(Guid portfolioId, Guid assetId)
        {
            // Check if asset exists within portfolio first
            if (await _portfolioService.GetAssetByIdAsync(portfolioId, assetId) == null)
            {
                return NotFound($"Asset with ID {assetId} not found in portfolio {portfolioId}.");
            }
            var transactions = await _portfolioService.GetTransactionsForAssetAsync(portfolioId, assetId);
            return Ok(transactions);
        }

        // GET: api/Portfolios/{portfolioId}/performance
        /// <summary>
        /// Retrieves the performance report for a specific portfolio over a date range.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="startDate">Optional: The start date for the performance calculation (YYYY-MM-DD).</param>
        /// <param name="endDate">Optional: The end date for the performance calculation (YYYY-MM-DD).</param>
        /// <returns>A PortfolioPerformanceDto object if found, otherwise 404 Not Found.</returns>
        [HttpGet("{portfolioId}/performance")]
        [ProducesResponseType(typeof(PortfolioPerformanceDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPortfolioPerformance(
            Guid portfolioId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var performance = await _portfolioService.GetPortfolioPerformanceAsync(portfolioId, startDate, endDate);
            if (performance == null)
            {
                return NotFound();
            }
            return Ok(performance);
        }
    
    }   
}
