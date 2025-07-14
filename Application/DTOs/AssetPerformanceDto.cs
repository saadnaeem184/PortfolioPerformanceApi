using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AssetPerformanceDto
    {
        public Guid AssetId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public decimal AverageCostBasis { get; set; }
        public decimal CurrentMarketPrice { get; set; }
        public decimal UnrealizedGainLoss { get; set; }
        public decimal RealizedGainLoss { get; set; }
    }
}
