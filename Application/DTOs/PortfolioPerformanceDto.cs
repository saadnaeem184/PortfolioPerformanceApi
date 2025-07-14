using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PortfolioPerformanceDto
    {
        public Guid PortfolioId { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public decimal CurrentTotalValue { get; set; }
        public decimal TotalRealizedGainLoss { get; set; }
        public decimal TotalUnrealizedGainLoss { get; set; }
        public Dictionary<string, decimal> AssetAllocation { get; set; } = new Dictionary<string, decimal>(); // Symbol -> Percentage
        public List<HistoricalValueDto> HistoricalValues { get; set; } = new List<HistoricalValueDto>();
    }
}
